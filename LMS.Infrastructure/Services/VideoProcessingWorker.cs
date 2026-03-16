using Microsoft.AspNetCore.SignalR;
using LMS.Infrastructure.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace LMS.Infrastructure.Services;

public class VideoProcessingWorker : BackgroundService
{
    private readonly IVideoProcessingQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingWorker> _logger;
    private readonly IFFmpegDownloader _ffmpegDownloader;

    public VideoProcessingWorker(
        IVideoProcessingQueue queue,
        IServiceProvider serviceProvider,
        ILogger<VideoProcessingWorker> logger,
        IFFmpegDownloader ffmpegDownloader)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _ffmpegDownloader = ffmpegDownloader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Video Processing Worker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var lessonId = await _queue.DequeueAsync(stoppingToken);
                _logger.LogInformation($"Processing video for Lesson ID: {lessonId}");

                await ProcessVideoAsync(lessonId, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing video processing task.");
            }
        }

        _logger.LogInformation("Video Processing Worker is stopping.");
    }

    private async Task ProcessVideoAsync(int lessonId, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<VideoHub>>();

        var lesson = await db.Lessons.FindAsync(lessonId);
        if (lesson == null || string.IsNullOrEmpty(lesson.VideoStorageKey)) return;

        lesson.VideoStatus = "Processing";
        await db.SaveChangesAsync();
        await hubContext.Clients.Group($"lesson_{lessonId}").SendAsync("VideoStatusUpdated", lessonId, lesson.VideoStatus, null);

        try
        {
            var ffmpegPath = await _ffmpegDownloader.GetFFmpegPathAsync();
            var ffprobePath = await _ffmpegDownloader.GetFFprobePathAsync();

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var inputPath = Path.Combine(wwwroot, "uploads", lesson.VideoStorageKey.TrimStart('/'));
            var outputDir = Path.Combine(wwwroot, "uploads", "hls", lessonId.ToString());
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            var m3u8Path = Path.Combine(outputDir, "index.m3u8");

            // 1. Get duration using ffprobe
            var duration = await GetVideoDurationAsync(ffprobePath, inputPath);
            lesson.VideoDurationSeconds = (int)duration;

            // 2. Convert to HLS using ffmpeg
            _logger.LogInformation($"Starting HLS conversion for Lesson {lessonId}. Input: {inputPath}");
            // Optimization: -threads 0 uses all cores, -preset ultrafast is the fastest x264 mode
            var arguments = $"-y -i \"{inputPath}\" -c copy -map 0 -f hls -hls_time 10 -hls_list_size 0 -hls_segment_filename \"{outputDir}/seg%d.ts\" \"{m3u8Path}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            var ffmpegLog = new System.Text.StringBuilder();
            process.ErrorDataReceived += (s, e) => { if (e.Data != null) ffmpegLog.AppendLine(e.Data); };

            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync(stoppingToken);

            if (process.ExitCode == 0)
            {
                lesson.VideoStatus = "Ready";
                lesson.VideoStorageUrl = $"/uploads/hls/{lessonId}/index.m3u8";
                _logger.LogInformation($"Video processed successfully for Lesson {lessonId}");
                
                // Xóa video gốc sau khi convert thành công
                if (File.Exists(inputPath)) 
                {
                    try { File.Delete(inputPath); }
                    catch (Exception ex) { _logger.LogWarning($"Could not delete original video {inputPath}: {ex.Message}"); }
                }
            }
            else
            {
                _logger.LogError($"FFmpeg error for Lesson {lessonId}: {ffmpegLog}");
                lesson.VideoStatus = "Failed";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception during video processing for Lesson {lessonId}");
            lesson.VideoStatus = "Failed";
        }

        await db.SaveChangesAsync();
        
        await hubContext.Clients.Group($"lesson_{lessonId}").SendAsync("VideoStatusUpdated", lessonId, lesson.VideoStatus, lesson.VideoStorageUrl);
    }

    private async Task<double> GetVideoDurationAsync(string ffprobePath, string inputPath)
    {
        var arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
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
        
        // Wait at most 30 seconds for ffprobe
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(cts.Token);

        if (double.TryParse(output.Trim(), out var duration))
        {
            return duration;
        }

        return 0;
    }
}
