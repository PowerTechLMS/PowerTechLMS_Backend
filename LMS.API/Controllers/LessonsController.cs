using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Infrastructure.Services;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/modules/{moduleId}/[controller]")]
[Authorize(Policy = "CourseEdit")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;
    private readonly IVideoProcessingQueue _videoQueue;
    private readonly ILogger<LessonsController> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public LessonsController(ILessonService lessonService, IVideoProcessingQueue videoQueue, ILogger<LessonsController> logger, IServiceScopeFactory scopeFactory)
    {
        _lessonService = lessonService;
        _videoQueue = videoQueue;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    [HttpPost]
    public async Task<ActionResult> Create(int moduleId, [FromBody] CreateLessonRequest request)
        => Ok(await _lessonService.CreateLessonAsync(moduleId, request));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLessonRequest request)
    {
        try { return Ok(await _lessonService.UpdateLessonAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _lessonService.DeleteLessonAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("sort-order")]
    public async Task<ActionResult> UpdateSortOrder([FromBody] UpdateSortOrderRequest request)
    {
        await _lessonService.UpdateSortOrderAsync(request.Items);
        return Ok();
    }

    [HttpPost("{lessonId}/attachments")]
    public async Task<ActionResult> UploadAttachment(int moduleId, int lessonId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "Tệp đính kèm không hợp lệ." });
        using var stream = file.OpenReadStream();
        return Ok(await _lessonService.UploadAttachmentAsync(lessonId, stream, file.FileName, file.Length));
    }

    [HttpPost("{id}/video")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult> UploadVideo(int moduleId, int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var url = await _lessonService.UploadVideoAsync(id, stream, file.FileName);
        // Traditional upload also enqueues for processing
        _videoQueue.Enqueue(id);
        return Ok(new { url });
    }

    [HttpPost("{id}/video-chunk")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult> UploadVideoChunk(int id, [FromForm] int chunkIndex, [FromForm] int totalChunks, [FromForm] string fileName, IFormFile file)
    {
        _logger.LogInformation("Received chunk {ChunkIndex}/{TotalChunks} for lesson {LessonId}", chunkIndex, totalChunks, id);

        var tempDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "temp", id.ToString());
        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

        var chunkPath = Path.Combine(tempDir, $"{chunkIndex}.chunk");
        using (var stream = new FileStream(chunkPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Check if all chunks are uploaded
        var uploadedChunks = Directory.GetFiles(tempDir, "*.chunk").Length;
        if (uploadedChunks == totalChunks)
        {
            _logger.LogInformation("All chunks received for lesson {LessonId}. Starting background merge...", id);
            
            var ext = Path.GetExtension(fileName);
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var finalFileName = $"v_{id}_{timestamp}{ext}";
            var videoUrl = $"/uploads/videos/{finalFileName}";

            // Run merge in background to unlock frontend instantly
            _ = Task.Run(async () =>
            {
                try
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var finalDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
                    if (!Directory.Exists(finalDir)) Directory.CreateDirectory(finalDir);
                    var finalPath = Path.Combine(finalDir, finalFileName);

                    using (var finalStream = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        for (int i = 0; i < totalChunks; i++)
                        {
                            var partPath = Path.Combine(tempDir, $"{i}.chunk");
                            using (var partStream = new FileStream(partPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                await partStream.CopyToAsync(finalStream);
                            }
                        }
                    }

                    sw.Stop();
                    _logger.LogInformation("Merge completed for lesson {LessonId} in {ElapsedMs}ms", id, sw.ElapsedMilliseconds);

                    // Clean up chunks
                    Directory.Delete(tempDir, true);

                    // Update Lesson & Enqueue
                    var storageKey = $"videos/{finalFileName}";
                    
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var scopedLessonService = scope.ServiceProvider.GetRequiredService<ILessonService>();
                        var scopedVideoQueue = scope.ServiceProvider.GetRequiredService<IVideoProcessingQueue>();
                        
                        await scopedLessonService.UpdateVideoMetadataAsync(id, storageKey, videoUrl);
                        scopedVideoQueue.Enqueue(id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in background merge for lesson {LessonId}", id);
                }
            });

            return Ok(new { url = videoUrl, status = "Merging" });
        }

        return Accepted(new { message = $"Chunk {chunkIndex} received." });
    }

    [HttpDelete("attachments/{attachmentId}")]
    public async Task<ActionResult> DeleteAttachment(int attachmentId)
    {
        try { await _lessonService.DeleteAttachmentAsync(attachmentId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id}/quiz")]
    public async Task<ActionResult> CreateLessonQuiz(int moduleId, int id, [FromBody] CreateQuizRequest request)
    {
        try
        {
            var quizId = await _lessonService.CreateLessonQuizAsync(id, request);
            return Ok(new { id = quizId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy bài học để thêm Quiz." });
        }
        catch (Exception ex)
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
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy tài liệu đính kèm." });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "Tệp vật lý không còn trên máy chủ." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
