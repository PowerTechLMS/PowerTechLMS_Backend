using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace LMS.Infrastructure.Services;

public class AiProcessingService : IAiProcessingService
{
    private readonly AppDbContext _db;
    private readonly ITranscriptionService _whisper;
    private readonly VectorDbService _vectorDb;
    private readonly TextExtractionService _textExtractor;
    private readonly ILogger<AiProcessingService> _logger;
    private readonly IHubContext<VideoHub> _hubContext;
    private readonly IFFmpegDownloader _ffmpegDownloader;

    public AiProcessingService(
        AppDbContext db,
        ITranscriptionService whisper,
        VectorDbService vectorDb,
        TextExtractionService textExtractor,
        ILogger<AiProcessingService> logger,
        IHubContext<VideoHub> hubContext,
        IFFmpegDownloader ffmpegDownloader)
    {
        _db = db;
        _whisper = whisper;
        _vectorDb = vectorDb;
        _textExtractor = textExtractor;
        _logger = logger;
        _hubContext = hubContext;
        _ffmpegDownloader = ffmpegDownloader;
    }

    public async Task ProcessLessonVideoAsync(int lessonId)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if(lesson == null || string.IsNullOrEmpty(lesson.VideoStorageUrl))
            return;

        var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var videoPath = Path.Combine(wwwroot, "uploads", lesson.VideoStorageKey?.TrimStart('/') ?? string.Empty);
        var audioDir = Path.Combine(wwwroot, "uploads", "audio");
        if(!Directory.Exists(audioDir))
            Directory.CreateDirectory(audioDir);

        var audioPath = Path.Combine(audioDir, $"{lessonId}.wav");

        if(File.Exists(videoPath))
        {
            await ExtractAudioAsync(videoPath, audioPath);
        } else if(!File.Exists(audioPath))
        {
            return;
        }

        _logger.LogInformation($"[AI] Bắt đầu Whisper Transcription cho bài học: {lessonId}");
        var segments = await _whisper.TranscribeAsync(audioPath);
        _logger.LogInformation($"[AI] Whisper hoàn tất: {segments.Count} segments tìm thấy.");

        var rawTexts = segments.Select(s => s.Text).ToList();

        var processedSegments = new List<(TextSegment Segment, string RefinedText)>();
        bool skipVectorDb = false;

        for(int i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            var refined = seg.Text;
            processedSegments.Add((seg, refined));

            if(!skipVectorDb)
            {
                try
                {
                    await _vectorDb.UpsertVectorAsync(
                        Guid.NewGuid(),
                        refined,
                        new { LessonId = lessonId, Type = "Video", Start = seg.StartTime });
                } catch
                {
                    skipVectorDb = true;
                }
            }
        }

        var srtContent = GenerateSrtContent(processedSegments);
        var vttContent = GenerateVttContent(processedSegments);

        var subtitleDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "subtitles");
        if(!Directory.Exists(subtitleDir))
            Directory.CreateDirectory(subtitleDir);

        var srtPath = Path.Combine(subtitleDir, $"{lessonId}.srt");
        var vttPath = Path.Combine(subtitleDir, $"{lessonId}.vtt");

        await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8);
        await File.WriteAllTextAsync(vttPath, vttContent, Encoding.UTF8);

        // Tạo Master Playlist để tích hợp phụ đề vào HLS
        var hlsDir = Path.Combine(wwwroot, "uploads", "hls", lessonId.ToString());
        if (Directory.Exists(hlsDir))
        {
            var masterM3u8Path = Path.Combine(hlsDir, "master.m3u8");
            var subtitleM3u8Path = Path.Combine(hlsDir, "subtitles.m3u8");

            // Sao chép phụ đề vào cùng thư mục HLS để tránh dùng đường dẫn tương đối phức tạp (../)
            var localSubtitlePath = Path.Combine(hlsDir, "subtitles.vtt");
            var sourceSubtitlePath = Path.Combine(wwwroot, "uploads", "subtitles", $"{lessonId}.vtt");
            if (File.Exists(sourceSubtitlePath))
            {
                File.Copy(sourceSubtitlePath, localSubtitlePath, true);
            }

            var utf8WithoutBom = new UTF8Encoding(false);

            // 1. Tạo Subtitle Playlist (VOD) theo chuẩn HLS
            var totalDuration = processedSegments.Count is not 0 ? processedSegments.Last().Segment.EndTime : 0;
            var subM3u8Content = new StringBuilder();
            subM3u8Content.AppendLine("#EXTM3U");
            subM3u8Content.AppendLine("#EXT-X-VERSION:3");
            subM3u8Content.AppendLine($"#EXT-X-TARGETDURATION:{(int)Math.Ceiling(totalDuration)}");
            subM3u8Content.AppendLine("#EXT-X-MEDIA-SEQUENCE:0");
            subM3u8Content.AppendLine("#EXT-X-PLAYLIST-TYPE:VOD");
            subM3u8Content.AppendLine($"#EXTINF:{totalDuration:F6},");
            subM3u8Content.AppendLine("subtitles.vtt");
            subM3u8Content.AppendLine("#EXT-X-ENDLIST");
            await File.WriteAllTextAsync(subtitleM3u8Path, subM3u8Content.ToString(), utf8WithoutBom);

            // 2. Tạo Master Playlist (HLS v6) để hỗ trợ phụ đề native
            var masterM3u8Content = new StringBuilder();
            masterM3u8Content.AppendLine("#EXTM3U");
            masterM3u8Content.AppendLine("#EXT-X-VERSION:6");
            masterM3u8Content.AppendLine("#EXT-X-MEDIA:TYPE=SUBTITLES,GROUP-ID=\"subs\",NAME=\"Tiếng Việt\",DEFAULT=YES,AUTOSELECT=YES,FORCED=NO,LANGUAGE=\"vi\",URI=\"subtitles.m3u8\"");
            masterM3u8Content.AppendLine("#EXT-X-STREAM-INF:BANDWIDTH=1280000,SUBTITLES=\"subs\"");
            masterM3u8Content.AppendLine("index.m3u8");

            await File.WriteAllTextAsync(masterM3u8Path, masterM3u8Content.ToString(), utf8WithoutBom);
            lesson.VideoStorageUrl = $"/uploads/hls/{lessonId}/master.m3u8";
        }

        lesson.Transcript = string.Join(" ", processedSegments.Select(x => x.RefinedText));
        lesson.SubtitlesPath = $"/uploads/subtitles/{lessonId}.vtt";
        lesson.IsAiProcessed = true;
        await _db.SaveChangesAsync();

        // Gửi thông báo cập nhật URL mới (master.m3u8) để frontend chuyển sang luồng có phụ đề
        await _hubContext.Clients.Group($"lesson_{lessonId}").SendAsync("VideoStatusUpdated", lessonId, "Ready", lesson.VideoStorageUrl);
        await _hubContext.Clients.Group($"lesson_{lessonId}").SendAsync("AiProcessingCompleted", lessonId);
    }

    public async Task ProcessDocumentAsync(int documentId)
    {
        var doc = await _db.Documents.Include(d => d.CurrentVersion).FirstOrDefaultAsync(d => d.Id == documentId);
        if(doc == null || doc.CurrentVersion == null)
            return;

        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            doc.CurrentVersion.StorageKey?.TrimStart('/') ?? string.Empty);
        var fullText = _textExtractor.ExtractText(filePath);

        var chunks = SplitText(fullText, 500);
        foreach(var chunk in chunks)
        {
            await _vectorDb.UpsertVectorAsync(Guid.NewGuid(), chunk, new { DocumentId = documentId, Type = "Document" });
        }

        doc.IsAiProcessed = true;
        await _db.SaveChangesAsync();
    }

    private List<string> SplitText(string text, int chunkSize)
    {
        var list = new List<string>();
        for(int i = 0; i < text.Length; i += chunkSize)
        {
            list.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        }
        return list;
    }

    public Task ProcessLessonAttachmentAsync(int attachmentId) { return Task.CompletedTask; }

    private string GenerateVttContent(List<(TextSegment Segment, string RefinedText)> processedSegments)
    {
        var sb = new StringBuilder();
        sb.AppendLine("WEBVTT");
        sb.AppendLine();
        foreach(var item in processedSegments)
        {
            sb.AppendLine($"{FormatVttTime(item.Segment.StartTime)} --> {FormatVttTime(item.Segment.EndTime)}");
            sb.AppendLine(item.RefinedText);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string GenerateSrtContent(List<(TextSegment Segment, string RefinedText)> processedSegments)
    {
        var sb = new StringBuilder();
        for(int i = 0; i < processedSegments.Count; i++)
        {
            var item = processedSegments[i];
            sb.AppendLine((i + 1).ToString());
            sb.AppendLine($"{FormatSrtTime(item.Segment.StartTime)} --> {FormatSrtTime(item.Segment.EndTime)}");
            sb.AppendLine(item.RefinedText);
            sb.AppendLine();
        }
        return sb.ToString();
    }

    private string FormatSrtTime(double seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return string.Format(
            "{0:00}:{1:00}:{2:00},{3:000}",
            t.Hours + (t.Days * 24),
            t.Minutes,
            t.Seconds,
            t.Milliseconds);
    }

    private string FormatVttTime(double seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return string.Format(
            "{0:00}:{1:00}:{2:00}.{3:000}",
            t.Hours + (t.Days * 24),
            t.Minutes,
            t.Seconds,
            t.Milliseconds);
    }

    private async Task ExtractAudioAsync(string videoPath, string audioPath)
    {
        var ffmpegPath = await _ffmpegDownloader.GetFFmpegPathAsync();
        var arguments = $"-y -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{audioPath}\"";

        using var process = new Process
        {
            StartInfo =
                new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }
        };

        process.Start();
        await process.WaitForExitAsync();

        if(process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"FFmpeg audio extraction failed: {error}");
        }
    }
}
