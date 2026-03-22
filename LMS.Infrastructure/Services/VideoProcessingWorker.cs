using Hangfire;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
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

        lesson.VideoStatus = "Processing";
        await db.SaveChangesAsync();
        await hubContext.Clients
            .Group($"lesson_{lessonId}")
            .SendAsync("VideoStatusUpdated", lessonId, lesson.VideoStatus, null);

        try
        {
            var ffmpegPath = await _ffmpegDownloader.GetFFmpegPathAsync();
            var ffprobePath = await _ffmpegDownloader.GetFFprobePathAsync();

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var inputPath = Path.Combine(wwwroot, "uploads", lesson.VideoStorageKey.TrimStart('/'));
            var outputDir = Path.Combine(wwwroot, "uploads", "hls", lessonId.ToString());
            if(!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var m3u8Path = Path.Combine(outputDir, "index.m3u8");

            var duration = await GetVideoDurationAsync(ffprobePath, inputPath);
            lesson.VideoDurationSeconds = (int)duration;

            var arguments = $"-y -i \"{inputPath}\" -c:v libx264 -preset ultrafast -crf 23 -c:a aac -ar 44100 -map 0 -f hls -hls_time 10 -hls_list_size 0 -hls_segment_filename \"{outputDir}/seg%d.ts\" \"{m3u8Path}\"";

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
            await process.WaitForExitAsync(stoppingToken);

            if(process.ExitCode == 0)
            {
                lesson.VideoStatus = "Ready";
                lesson.VideoStorageUrl = $"/uploads/hls/{lessonId}/index.m3u8";

                var audioDir = Path.Combine(wwwroot, "uploads", "audio");
                if(!Directory.Exists(audioDir))
                    Directory.CreateDirectory(audioDir);
                var audioPath = Path.Combine(audioDir, $"{lessonId}.wav");
                await ExtractAudioForAiAsync(ffmpegPath, inputPath, audioPath, stoppingToken);

                BackgroundJob.Enqueue<IAiProcessingService>(x => x.ProcessLessonVideoAsync(lessonId));

                if(File.Exists(inputPath))
                {
                    File.Delete(inputPath);
                }
            } else
            {
                lesson.VideoStatus = "Failed";
            }
        } catch
        {
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

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        await process.WaitForExitAsync(cts.Token);

        if(double.TryParse(output.Trim(), out var duration))
        {
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
