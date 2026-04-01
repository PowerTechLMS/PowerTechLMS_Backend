using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
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
    private readonly ILlmService _llm;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

    public AiProcessingService(
        AppDbContext db,
        ITranscriptionService whisper,
        VectorDbService vectorDb,
        TextExtractionService textExtractor,
        ILogger<AiProcessingService> logger,
        IHubContext<VideoHub> hubContext,
        IFFmpegDownloader ffmpegDownloader,
        ILlmService llm,
        Microsoft.Extensions.Configuration.IConfiguration config)
    {
        _db = db;
        _whisper = whisper;
        _vectorDb = vectorDb;
        _textExtractor = textExtractor;
        _logger = logger;
        _hubContext = hubContext;
        _ffmpegDownloader = ffmpegDownloader;
        _llm = llm;
        _config = config;
    }

    public async Task ProcessLessonVideoAsync(int lessonId)
    {
        _logger.LogInformation("[AI] Bắt đầu xử lý hậu kỳ cho Bài học: {LessonId}", lessonId);
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if(lesson == null || string.IsNullOrEmpty(lesson.VideoStorageUrl))
            return;

        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot) 
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;
        var videoPath = Path.Combine(wwwroot, "uploads", lesson.VideoStorageKey?.TrimStart('/') ?? string.Empty);
        var audioDir = Path.Combine(wwwroot, "uploads", "audio");
        if(!Directory.Exists(audioDir))
            Directory.CreateDirectory(audioDir);

        var audioPath = Path.Combine(audioDir, $"{lessonId}.wav");

        _logger.LogInformation("[AI] Đang kiểm tra file âm thanh/video để trích xuất...");
        if(File.Exists(videoPath))
        {
            await ExtractAudioAsync(videoPath, audioPath);
        } else if(!File.Exists(audioPath))
        {
            _logger.LogWarning("[AI] Không tìm thấy audio file {AudioPath} và video file {VideoPath}", audioPath, videoPath);
            return;
        }

        _logger.LogInformation("[AI] Đang gỡ băng (Whisper) cho bài học {LessonId}...", lessonId);
        await _vectorDb.DeleteVectorsByFilterAsync("LessonId", lessonId);

        var segments = await _whisper.TranscribeAsync(audioPath);
        _logger.LogInformation("[AI] Hoàn tất gỡ băng. Số segments: {Count}", segments.Count);


        var rawTexts = segments.Select(s => s.Text).ToList();

        var processedSegments = new List<(TextSegment Segment, string RefinedText)>();
        bool skipVectorDb = false;

        _logger.LogInformation("[AI] Đang lưu vector search...");
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
                        new Dictionary<string, object>
                        { { "LessonId", lessonId }, { "Type", "Video" }, { "Start", seg.StartTime } });
                } catch
                {
                    skipVectorDb = true;
                }
            }
        }

        try
        {
            if(File.Exists(audioPath))
            {
                File.Delete(audioPath);
            }
        } catch(Exception ex)
        {
            _logger.LogWarning($"[AI] Không thể xoá file âm thanh tạm: {ex.Message}");
        }

        _logger.LogInformation("[AI] Đang tạo file phụ đề .srt và .vtt...");
        var srtContent = GenerateSrtContent(processedSegments);
        var vttContent = GenerateVttContent(processedSegments);

        var subtitleDir = Path.Combine(wwwroot, "uploads", "subtitles");
        if(!Directory.Exists(subtitleDir))
            Directory.CreateDirectory(subtitleDir);

        var srtPath = Path.Combine(subtitleDir, $"{lessonId}.srt");
        var vttPath = Path.Combine(subtitleDir, $"{lessonId}.vtt");

        await File.WriteAllTextAsync(srtPath, srtContent, Encoding.UTF8);
        await File.WriteAllTextAsync(vttPath, vttContent, Encoding.UTF8);

        _logger.LogInformation("[AI] Cập nhật HLS master playlist tích hợp phụ đề...");
        var hlsDir = Path.Combine(wwwroot, "uploads", "hls", lessonId.ToString());
        if(Directory.Exists(hlsDir))
        {
            var masterM3u8Path = Path.Combine(hlsDir, "master.m3u8");
            var subtitleM3u8Path = Path.Combine(hlsDir, "subtitles.m3u8");

            var localVttPath = Path.Combine(hlsDir, "subtitles.vtt");
            var localSrtPath = Path.Combine(hlsDir, "subtitles.srt");
            
            var sourceVttPath = Path.Combine(wwwroot, "uploads", "subtitles", $"{lessonId}.vtt");
            var sourceSrtPath = Path.Combine(wwwroot, "uploads", "subtitles", $"{lessonId}.srt");

            if(File.Exists(sourceVttPath))
            {
                File.Copy(sourceVttPath, localVttPath, true);
            }
            if(File.Exists(sourceSrtPath))
            {
                File.Copy(sourceSrtPath, localSrtPath, true);
            }

            var utf8WithoutBom = new UTF8Encoding(false);

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

            var masterM3u8Content = new StringBuilder();
            masterM3u8Content.AppendLine("#EXTM3U");
            masterM3u8Content.AppendLine("#EXT-X-VERSION:6");
            masterM3u8Content.AppendLine(
                "#EXT-X-MEDIA:TYPE=SUBTITLES,GROUP-ID=\"subs\",NAME=\"Tiếng Việt\",DEFAULT=YES,AUTOSELECT=YES,FORCED=NO,LANGUAGE=\"vi\",URI=\"subtitles.m3u8\"");
            masterM3u8Content.AppendLine("#EXT-X-STREAM-INF:BANDWIDTH=1280000,SUBTITLES=\"subs\"");
            masterM3u8Content.AppendLine("index.m3u8");

            await File.WriteAllTextAsync(masterM3u8Path, masterM3u8Content.ToString(), utf8WithoutBom);
            lesson.VideoStorageUrl = $"/uploads/hls/{lessonId}/master.m3u8";
        }

        lesson.Transcript = string.Join(" ", processedSegments.Select(x => x.RefinedText));
        lesson.SubtitlesPath = $"/uploads/subtitles/{lessonId}.vtt";
        lesson.IsAiProcessed = true;

        if(!string.IsNullOrEmpty(lesson.Transcript))
        {
            try
            {
                var summaryPrompt = "Bạn là một trợ lý giáo dục. Dưới đây là bản gỡ băng (transcript) của một bài giảng video. " +
                    "Hãy viết một bản tóm tắt ngắn gọn, súc tích (khoảng 3-5 gạch đầu dòng) về các nội dung chính của bài giảng này.\n\n" +
                    "Transcript:\n" +
                    lesson.Transcript;
                lesson.AiSummary = await _llm.GenerateResponseAsync(summaryPrompt, "Hãy tóm tắt bài giảng này.");
            } catch(Exception ex)
            {
                _logger.LogError($"[AI] Lỗi khi tạo tóm tắt cho bài học {lessonId}: {ex.Message}");
            }
        }

        await _db.SaveChangesAsync();

        await _hubContext.Clients
            .Group($"lesson_{lessonId}")
            .SendAsync("VideoStatusUpdated", lessonId, "Ready", lesson.VideoStorageUrl);
        await _hubContext.Clients.Group($"lesson_{lessonId}").SendAsync("AiProcessingCompleted", lessonId);
    }

    public async Task ProcessDocumentAsync(int documentId)
    {
        var doc = await _db.Documents.Include(d => d.CurrentVersion).FirstOrDefaultAsync(d => d.Id == documentId);
        if(doc == null || doc.CurrentVersion == null)
            return;

        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot) 
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var filePath = Path.Combine(
            wwwroot,
            "uploads",
            doc.CurrentVersion.StorageKey?.TrimStart('/') ?? string.Empty);
        var fullText = _textExtractor.ExtractText(filePath);

        var chunks = SplitText(fullText, 1000, 200);

        await _vectorDb.DeleteVectorsByFilterAsync("DocumentId", documentId);

        foreach(var chunk in chunks)
        {
            await _vectorDb.UpsertVectorAsync(
                Guid.NewGuid(),
                chunk,
                new Dictionary<string, object> { { "DocumentId", documentId }, { "Type", "Document" } });
        }


        doc.IsAiProcessed = true;
        await _db.SaveChangesAsync();
    }

    private List<string> SplitText(string text, int chunkSize, int overlap = 100)
    {
        var list = new List<string>();
        if(text.Length <= chunkSize)
        {
            list.Add(text);
            return list;
        }

        for(int i = 0; i < text.Length; i += (chunkSize - overlap))
        {
            var length = Math.Min(chunkSize, text.Length - i);
            list.Add(text.Substring(i, length));
            if(i + length >= text.Length)
                break;
        }
        return list;
    }

    public async Task ProcessLessonAttachmentAsync(int attachmentId)
    {
        var attachment = await _db.LessonAttachments.FindAsync(attachmentId);
        if(attachment == null || string.IsNullOrEmpty(attachment.StorageKey))
            return;

        try
        {
            var storageRoot = _config["Storage:RootPath"];
            var wwwroot = string.IsNullOrEmpty(storageRoot) 
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : storageRoot;

            var filePath = Path.Combine(
                wwwroot,
                "uploads",
                attachment.StorageKey.TrimStart('/'));
            if(!File.Exists(filePath))
                return;

            var fullText = _textExtractor.ExtractText(filePath);
            if(!string.IsNullOrWhiteSpace(fullText))
            {
                var chunks = SplitText(fullText, 1000, 200);

                await _vectorDb.DeleteVectorsByFilterAsync("AttachmentId", attachmentId);

                foreach(var chunk in chunks)
                {
                    try
                    {
                        await _vectorDb.UpsertVectorAsync(
                            Guid.NewGuid(),
                            chunk,
                            new Dictionary<string, object>
                            {
                            { "LessonId", attachment.LessonId },
                            { "AttachmentId", attachmentId },
                            { "Type", "Attachment" },
                            { "FileName", attachment.FileName }
                            });
                    } catch(Exception ex)
                    {
                        _logger.LogError(
                            $"[AI] Lỗi khi tạo vector cho một chunk của tài liệu {attachmentId}: {ex.Message}");
                    }
                }
            }
        } catch(Exception ex)
        {
            _logger.LogError($"[AI] Lỗi nghiêm trọng khi xử lý tài liệu {attachmentId}: {ex.Message}");
        } finally
        {
            attachment.IsAiProcessed = true;
            await _db.SaveChangesAsync();
        }
    }


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

    public async Task ProcessLessonTextAsync(int lessonId)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        if(lesson == null || string.IsNullOrWhiteSpace(lesson.Content))
            return;

        await _vectorDb.DeleteVectorsByFilterAsync("LessonId", lessonId);
        var chunks = SplitText(lesson.Content, 1000, 200);

        foreach(var chunk in chunks)
        {
            await _vectorDb.UpsertVectorAsync(
                Guid.NewGuid(),
                chunk,
                new Dictionary<string, object> { { "LessonId", lessonId }, { "Type", "Text" } });
        }

        lesson.IsAiProcessed = true;
        await _db.SaveChangesAsync();
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
