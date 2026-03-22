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
    private readonly IAiModelService _protonX;
    private readonly VectorDbService _vectorDb;
    private readonly TextExtractionService _textExtractor;
    private readonly ILogger<AiProcessingService> _logger;
    private readonly IHubContext<VideoHub> _hubContext;
    private readonly IFFmpegDownloader _ffmpegDownloader;

    public AiProcessingService(
        AppDbContext db,
        ITranscriptionService whisper,
        IAiModelService protonX,
        VectorDbService vectorDb,
        TextExtractionService textExtractor,
        ILogger<AiProcessingService> logger,
        IHubContext<VideoHub> hubContext,
        IFFmpegDownloader ffmpegDownloader)
    {
        _db = db;
        _whisper = whisper;
        _protonX = protonX;
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

        try
        {
            Console.WriteLine($"[AI] Bắt đầu xử lý cho Lesson: {lessonId}");
            _logger.LogInformation($"Bắt đầu xử lý AI cho video bài học: {lessonId}");

            var wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var videoPath = Path.Combine(wwwroot, "uploads", lesson.VideoStorageKey?.TrimStart('/') ?? string.Empty);
            var audioDir = Path.Combine(wwwroot, "uploads", "audio");
            if(!Directory.Exists(audioDir))
                Directory.CreateDirectory(audioDir);

            var audioPath = Path.Combine(audioDir, $"{lessonId}.wav");

            if(File.Exists(videoPath))
            {
                Console.WriteLine($"[AI] Đang trích xuất audio từ: {videoPath}");
                _logger.LogInformation($"Trích xuất audio từ {videoPath} sang {audioPath}");
                await ExtractAudioAsync(videoPath, audioPath);
            } else if(!File.Exists(audioPath))
            {
                Console.WriteLine($"[AI] KHÔNG TÌM THẤY VIDEO GỐC: {videoPath}");
                _logger.LogWarning(
                    $"Không tìm thấy video gốc ({videoPath}) cũng như audio ({audioPath}). Hủy xử lý AI.");
                return;
            }

            Console.WriteLine($"[AI] Đang chạy Whisper transcription cho: {audioPath}");
            var segments = await _whisper.TranscribeAsync(audioPath);
            Console.WriteLine($"[AI] Số lượng segments tìm thấy: {segments.Count}");

            var rawTexts = segments.Select(s => s.Text).ToList();
            var refinedTexts = await _protonX.RefineTextBatchAsync(rawTexts);

            var processedSegments = new List<(TextSegment Segment, string RefinedText)>();
            bool skipVectorDb = false;

            for(int i = 0; i < segments.Count; i++)
            {
                var seg = segments[i];
                var refined = i < refinedTexts.Count ? refinedTexts[i] : seg.Text;
                processedSegments.Add((seg, refined));

                if(!skipVectorDb)
                {
                    try
                    {
                        await _vectorDb.UpsertVectorAsync(
                            Guid.NewGuid(),
                            refined,
                            new { LessonId = lessonId, Type = "Video", Start = seg.StartTime });
                    } catch(Exception ex)
                    {
                        _logger.LogWarning(
                            $"[VectorDB] Tạm ngừng Vectorize cho bài học {lessonId} do lỗi: {ex.Message}");
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

            lesson.Transcript = string.Join(" ", processedSegments.Select(x => x.RefinedText));
            lesson.SubtitlesPath = $"/uploads/subtitles/{lessonId}.vtt";
            lesson.IsAiProcessed = true;
            await _db.SaveChangesAsync();
            await _hubContext.Clients.Group($"lesson_{lessonId}").SendAsync("AiProcessingCompleted", lessonId);
        } catch(Exception ex)
        {
            Console.WriteLine($"[AI] LỖI CỰC NGHIÊM TRỌNG: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            _logger.LogError(ex, $"Lỗi xử lý AI video cho bài học {lessonId}");
        }
    }

    public async Task ProcessDocumentAsync(int documentId)
    {
        var doc = await _db.Documents.Include(d => d.CurrentVersion).FirstOrDefaultAsync(d => d.Id == documentId);
        if(doc == null || doc.CurrentVersion == null)
            return;

        _logger.LogInformation($"Bắt đầu xử lý AI cho tài liệu: {documentId}");

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
