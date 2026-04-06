using Hangfire;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace LMS.Infrastructure.Services;

public class VideoProcessingWorker : BackgroundService
{
    private readonly IVideoProcessingQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VideoProcessingWorker> _logger;
    private readonly IFFmpegDownloader _ffmpegDownloader;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

    public VideoProcessingWorker(
        IVideoProcessingQueue queue,
        IServiceProvider serviceProvider,
        ILogger<VideoProcessingWorker> logger,
        IFFmpegDownloader ffmpegDownloader,
        Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _ffmpegDownloader = ffmpegDownloader;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var lessonId = await _queue.DequeueAsync(stoppingToken);
                await ProcessVideoAsync(lessonId, stoppingToken);
            } catch(OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessVideoAsync(int lessonId, CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<VideoHub>>();

        var lesson = await db.Lessons.FindAsync(lessonId);
        if(lesson == null || string.IsNullOrEmpty(lesson.VideoStorageKey))
            return;

        _logger.LogInformation("[VideoWorker] Bắt đầu xử lý cho LessonId: {LessonId}", lessonId);
        lesson.VideoStatus = "Processing";
        await db.SaveChangesAsync();
        await hubContext.Clients
            .Group($"lesson_{lessonId}")
            .SendAsync("VideoStatusUpdated", lessonId, lesson.VideoStatus, null);

        try
        {
            _logger.LogInformation("[VideoWorker] Đang lấy đường dẫn FFmpeg/FFprobe...");
            var ffmpegPath = await _ffmpegDownloader.GetFFmpegPathAsync();
            var ffprobePath = await _ffmpegDownloader.GetFFprobePathAsync();

            var storageRoot = _config["Storage:RootPath"];
            var wwwroot = string.IsNullOrEmpty(storageRoot) 
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : storageRoot;
            var inputPath = Path.Combine(wwwroot, "uploads", lesson.VideoStorageKey.TrimStart('/'));
            var outputDir = Path.Combine(wwwroot, "uploads", "hls", lessonId.ToString());
            if(!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var m3u8Path = Path.Combine(outputDir, "index.m3u8");

            _logger.LogInformation("[VideoWorker] Đang lấy thời lượng video cho: {InputPath}", inputPath);
            var duration = await GetVideoDurationAsync(ffprobePath, inputPath);
            lesson.VideoDurationSeconds = (int)duration;
            _logger.LogInformation("[VideoWorker] Thời lượng video: {Duration}s", duration);

            var arguments = $"-y -i \"{inputPath}\" -c:v libx264 -preset fast -vf \"scale='min(1920,iw)':-2\" -c:a aac -b:a 128k -ac 2 -f hls -hls_time 10 -hls_list_size 0 -hls_flags independent_segments -hls_segment_filename \"{outputDir}/seg%d.ts\" \"{m3u8Path}\"";

            var process = new Process
            {
                StartInfo =
                    new ProcessStartInfo
                    {
                        FileName = ffmpegPath,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
            };

            var ffmpegLog = new StringBuilder();
            process.ErrorDataReceived += (s, e) =>
            {
                if(e.Data != null)
                    ffmpegLog.AppendLine(e.Data);
            };

            process.Start();
            process.BeginErrorReadLine();
            _logger.LogInformation("[VideoWorker] Đang chờ FFmpeg hoàn tất...");
            await process.WaitForExitAsync(stoppingToken);

            _logger.LogInformation("[VideoWorker] FFmpeg kết thúc với ExitCode: {ExitCode}", process.ExitCode);

            if(process.ExitCode == 0)
            {
                _logger.LogInformation("[VideoWorker] Chuyển đổi HLS thành công cho LessonId: {LessonId}", lessonId);
                lesson.VideoStatus = "Ready";
                lesson.VideoStorageUrl = $"/uploads/hls/{lessonId}/index.m3u8";

                var audioDir = Path.Combine(wwwroot, "uploads", "audio");
                if(!Directory.Exists(audioDir))
                    Directory.CreateDirectory(audioDir);
                var audioPath = Path.Combine(audioDir, $"{lessonId}.wav");
                
                _logger.LogInformation("[VideoWorker] Đang trích xuất âm thanh cho AI: {AudioPath}", audioPath);
                await ExtractAudioForAiAsync(ffmpegPath, inputPath, audioPath, stoppingToken);

                _logger.LogInformation("[VideoWorker] Đang đẩy vào Hangfire để xử lý AI...");
                BackgroundJob.Enqueue<IAiProcessingService>(x => x.ProcessLessonVideoAsync(lessonId));

                if(File.Exists(inputPath))
                {
                    File.Delete(inputPath);
                }
            } else
            {
                _logger.LogError("[VideoWorker] FFmpeg HLS thất bại. Chi tiết lỗi:\n{FFmpegLog}", ffmpegLog.ToString());
                lesson.VideoStatus = "Failed";
            }
        } catch(Exception ex)
        {
            _logger.LogError(ex, "[VideoWorker] Lỗi nghiêm trọng khi xử lý video LessonId: {LessonId}", lessonId);
            lesson.VideoStatus = "Failed";
        }

        await db.SaveChangesAsync();

        await hubContext.Clients
            .Group($"lesson_{lessonId}")
            .SendAsync("VideoStatusUpdated", lessonId, lesson.VideoStatus, lesson.VideoStorageUrl);
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
        await process.WaitForExitAsync();

        var cleanOutput = output.Trim().Replace("\"", "");

        if (double.TryParse(cleanOutput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var duration))
        {
            if (duration > 1000000) 
            {
                if (duration > 1000000000) return duration / 1000000;
                if (duration > 10000000 && !cleanOutput.Contains(".")) return duration / 1000000;
                if (duration > 36000) return duration / 1000;
                return duration;
            }
            if (duration > 10000) 
            {
                 return duration / 1000;
            }

            return duration;
        }

        return 0;
    }

    private async Task ExtractAudioForAiAsync(
        string ffmpegPath,
        string inputPath,
        string outputPath,
        CancellationToken stoppingToken)
    {
        var arguments = $"-y -i \"{inputPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputPath}\"";
        using var process = new Process
        {
            StartInfo =
                new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
        };

        process.Start();
        await process.WaitForExitAsync(stoppingToken);
    }
}
