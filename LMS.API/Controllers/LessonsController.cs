using Hangfire;
using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/modules/{moduleId}/[controller]")]
[Authorize(Policy = "CourseEdit")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly IAiProcessingService _aiService;
    private readonly IVideoProcessingQueue _videoQueue;
    private readonly ILogger<LessonsController> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly IFFmpegDownloader _ffmpegDownloader;

    public LessonsController(
        ILessonService lessonService,
        IAiProcessingService aiService,
        IVideoProcessingQueue videoQueue,
        ILogger<LessonsController> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        IFFmpegDownloader ffmpegDownloader)
    {
        _lessonService = lessonService;
        _aiService = aiService;
        _videoQueue = videoQueue;
        _logger = logger;
        _scopeFactory = scopeFactory;
        _config = config;
        _ffmpegDownloader = ffmpegDownloader;
    }

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }

    private bool IsAdmin => User.IsInRole("Admin") ||
        User.IsInRole("Quản trị viên") ||
        User.HasClaim("permission", "user.manage");

    [HttpPost]
    public async Task<ActionResult> Create(int moduleId, [FromBody] CreateLessonRequest request) => Ok(
        await _lessonService.CreateLessonAsync(moduleId, request, UserId, IsAdmin));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int moduleId, int id, [FromBody] UpdateLessonRequest request)
    {
        try
        {
            return Ok(await _lessonService.UpdateLessonAsync(moduleId, id, request, UserId, IsAdmin));
        } catch(KeyNotFoundException)
        {
            return NotFound();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int moduleId, int id)
    {
        try
        {
            await _lessonService.DeleteLessonAsync(moduleId, id, UserId, IsAdmin);
            return NoContent();
        } catch(KeyNotFoundException)
        {
            return NotFound();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("sort-order")]
    public async Task<ActionResult> UpdateSortOrder(int moduleId, [FromBody] UpdateSortOrderRequest request)
    {
        try
        {
            await _lessonService.UpdateSortOrderAsync(moduleId, request.Items, UserId, IsAdmin);
            return Ok();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{lessonId}/attachments")]
    public async Task<ActionResult> UploadAttachment(int moduleId, int lessonId, IFormFile file)
    {
        if(file == null || file.Length == 0)
            return BadRequest(new { message = "Tệp đính kèm không hợp lệ." });
        try
        {
            using var stream = file.OpenReadStream();
            return Ok(
                await _lessonService.UploadAttachmentAsync(moduleId, lessonId, stream, file.FileName, UserId, IsAdmin));
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{id}/video")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult> UploadVideo(int moduleId, int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var url = await _lessonService.UploadVideoAsync(id, stream, file.FileName);
        _videoQueue.Enqueue(id);
        return Ok(new { url });
    }

    [HttpPost("{id}/video-chunk")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult> UploadVideoChunk(
        int id,
        [FromForm] int chunkIndex,
        [FromForm] int totalChunks,
        [FromForm] string fileName,
        IFormFile file)
    {
        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var tempDir = Path.Combine(wwwroot, "uploads", "temp", id.ToString());
        if(!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        var chunkPath = Path.Combine(tempDir, $"{chunkIndex}.chunk");
        using(var stream = new FileStream(chunkPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var uploadedChunks = Directory.GetFiles(tempDir, "*.chunk").Length;
        if(uploadedChunks == totalChunks)
        {
            var ext = Path.GetExtension(fileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var finalFileName = $"v_{id}_{timestamp}{ext}";
            var videoUrl = $"/uploads/videos/{finalFileName}";

            _ = Task.Run(
                async () =>
                {
                    var sw = Stopwatch.StartNew();
                    var storageRoot = _config["Storage:RootPath"];
                    var wwwroot = string.IsNullOrEmpty(storageRoot)
                        ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                        : storageRoot;

                    var finalDir = Path.Combine(wwwroot, "uploads", "videos");
                    if(!Directory.Exists(finalDir))
                        Directory.CreateDirectory(finalDir);
                    var finalPath = Path.Combine(finalDir, finalFileName);

                    using(var finalStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        for(int i = 0; i < totalChunks; i++)
                        {
                            var partPath = Path.Combine(tempDir, $"{i}.chunk");
                            using(var partStream = new FileStream(
                                partPath,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read))
                            {
                                await partStream.CopyToAsync(finalStream);
                            }
                        }
                    }

                    sw.Stop();

                    Directory.Delete(tempDir, true);

                    var storageKey = $"videos/{finalFileName}";

                    using(var scope = _scopeFactory.CreateScope())
                    {
                        var scopedLessonService = scope.ServiceProvider.GetRequiredService<ILessonService>();
                        var scopedVideoQueue = scope.ServiceProvider.GetRequiredService<IVideoProcessingQueue>();

                        await scopedLessonService.UpdateVideoMetadataAsync(id, storageKey, videoUrl);
                        scopedVideoQueue.Enqueue(id);

                        BackgroundJob.Enqueue<IAiProcessingService>(x => x.ProcessLessonVideoAsync(id));
                    }
                });

            return Ok(new { url = videoUrl, status = "Merging" });
        }

        return Accepted(new { message = $"Chunk {chunkIndex} received." });
    }

    [HttpDelete("{lessonId}/attachments/{attachmentId}")]
    public async Task<ActionResult> DeleteAttachment(int moduleId, int lessonId, int attachmentId)
    {
        try
        {
            await _lessonService.DeleteAttachmentAsync(moduleId, attachmentId, UserId, IsAdmin);
            return NoContent();
        } catch(KeyNotFoundException)
        {
            return NotFound();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{id}/quiz")]
    public async Task<ActionResult> CreateLessonQuiz(int moduleId, int id, [FromBody] CreateQuizRequest request)
    {
        try
        {
            var quizId = await _lessonService.CreateLessonQuizAsync(id, request);
            return Ok(new { id = quizId });
        } catch(KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy bài học để thêm Quiz." });
        } catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("attachments/{attachmentId}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadAttachment(int moduleId, int attachmentId)
    {
        try
        {
            var (stream, fileName, contentType) = await _lessonService.GetAttachmentFileAsync(attachmentId);
            return File(stream, contentType, fileName);
        } catch(KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy tài liệu đính kèm." });
        } catch(FileNotFoundException)
        {
            return NotFound(new { message = "Tệp vật lý không còn trên máy chủ." });
        } catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sync-all-durations")]
    [Authorize(Roles = "Admin,Quản trị viên")]
    public async Task<ActionResult> SyncAllDurations()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var lessons = db.Lessons
            .Where(
                l => l.Type == "Video" &&
                    (l.VideoDurationSeconds <= 0 || l.VideoDurationSeconds > 10000) &&
                    !string.IsNullOrEmpty(l.VideoStorageUrl))
            .ToList();

        int updatedCount = 0;
        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var ffprobePath = await _ffmpegDownloader.GetFFprobePathAsync();

        foreach(var lesson in lessons)
        {
            try
            {
                var filePath = Path.Combine(wwwroot, lesson.VideoStorageUrl?.TrimStart('/') ?? string.Empty);
                if(!System.IO.File.Exists(filePath))
                    continue;

                var duration = await GetVideoDurationAsync(ffprobePath, filePath);
                if(duration > 0)
                {
                    lesson.VideoDurationSeconds = (int)duration;
                    updatedCount++;
                }
            } catch(Exception ex)
            {
                _logger.LogWarning("Không thể lấy thời lượng cho bài học {Id}: {Msg}", lesson.Id, ex.Message);
            }
        }

        if(updatedCount > 0)
        {
            await db.SaveChangesAsync();
        }

        return Ok(
            new { message = $"Đã cập nhật thời lượng cho {updatedCount} bài học.", totalProcessed = lessons.Count });
    }

    private async Task<double> GetVideoDurationAsync(string ffprobePath, string inputPath)
    {
        var arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"";
        var process = new Process
        {
            StartInfo =
                new ProcessStartInfo
                {
                    FileName = ffprobePath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        var cleanOutput = output.Trim().Replace("\"", string.Empty);
        if(double.TryParse(cleanOutput, NumberStyles.Any, CultureInfo.InvariantCulture, out var duration))
        {
            if(duration > 1000000)
            {
                if(duration > 1000000000)
                    return duration / 1000000;
                if(duration > 10000000 && !cleanOutput.Contains("."))
                    return duration / 1000000;
                if(duration > 36000)
                    return duration / 1000;
                return duration;
            }
            if(duration > 10000)
            {
                return duration / 1000;
            }
            return duration;
        }
        return 0;
    }
}
