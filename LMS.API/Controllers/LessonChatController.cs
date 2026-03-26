using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LessonChatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly VectorDbService _vectorDb;
    private readonly ILlmService _llm;
    private readonly ILogger<LessonChatController> _logger;

    public LessonChatController(
        AppDbContext db,
        VectorDbService vectorDb,
        ILlmService llm,
        ILogger<LessonChatController> logger)
    {
        _db = db;
        _vectorDb = vectorDb;
        _llm = llm;
        _logger = logger;
    }

    [HttpGet("{lessonId}")]
    public async Task<IActionResult> GetChatHistory(int lessonId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var history = await _db.LessonChats
            .Where(c => c.LessonId == lessonId && c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(history);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var lesson = await _db.Lessons.FindAsync(request.LessonId);
        if(lesson is null)
            return NotFound("Không tìm thấy bài học.");

        var searchResults = await _vectorDb.SearchAsync(request.Message, request.LessonId, limit: 20);

        var expandedContexts = new HashSet<string>();

        var allSegments = await _vectorDb.GetAllSegmentsAsync(request.LessonId);
        var sortedVideoSegments = allSegments
            .Where(s => s.Metadata.Contains("\"Type\":\"Video\"") || s.Metadata.Contains("Type = Video"))
            .Select(s => new { s.Content, Timestamp = ExtractTimestamp(s.Metadata) })
            .OrderBy(s => s.Timestamp)
            .ToList();

        foreach(var res in searchResults)
        {
            var metaDict = TryParseMetadata(res.Metadata);
            var type = metaDict.GetValueOrDefault("Type")?.ToString();

            if(type == "Video")
            {
                var currentTS = ExtractTimestamp(res.Metadata);
                var index = sortedVideoSegments.FindIndex(s => Math.Abs(s.Timestamp - currentTS) < 0.1);

                if(index != -1)
                {
                    for(int i = index; i <= index + 2 && i < sortedVideoSegments.Count; i++)
                    {
                        var seg = sortedVideoSegments[i];
                        expandedContexts.Add(
                            $"- Nội dung bài giảng (Video tại [[{FormatTime(seg.Timestamp)}]]): {seg.Content}");
                    }
                } else
                {
                    expandedContexts.Add(
                        $"- Nội dung bài giảng: {res.Content} (Thời gian: [[{FormatTime(currentTS)}]])");
                }
            } else if(type == "Attachment")
            {
                var fileName = metaDict.GetValueOrDefault("FileName")?.ToString() ?? "Tài liệu đính kèm";
                expandedContexts.Add($"- Trích dẫn từ tài liệu [{fileName}]: {res.Content}");
            } else
            {
                expandedContexts.Add($"- Nội dung bổ sung: {res.Content}");
            }
        }

        var contextText = string.Join("\n", expandedContexts);

        var systemPrompt = 
            "Bạn là một trợ lý học tập thông minh của hệ thống PowerTech. " +
            "Dưới đây là thông tin trích xuất từ VIDEO bài giảng và các TÀI LIỆU đính kèm. " +
            "Hãy phân tích kỹ tất cả các nguồn dữ liệu này để trả lời câu hỏi của người dùng.\n\n" +
            "QUY TẮC PHẢN HỒI:\n" +
            "1. Nếu thông tin nằm trong TÀI LIỆU (PDF/Word), hãy trích dẫn rõ tên tài liệu.\n" +
            "2. Nếu thông tin nằm trong VIDEO, hãy định dạng dấu thời gian theo kiểu [[MM:SS]] hoặc [[HH:MM:SS]].\n" +
            "3. Luôn ưu tiên câu trả lời đầy đủ và chính xác dựa trên tài liệu trước, sau đó mới đến video.\n" +
            "4. Nếu nội dung không có trong bất kỳ tài liệu hay bài giảng nào, hãy trả lời lịch sự rằng bạn không tìm thấy thông tin cụ thể.\n\n" +
            "DỮ LIỆU BÀI HỌC CUNG CẤP:\n" +
            contextText;

        var aiResponse = await _llm.GenerateResponseAsync(systemPrompt, request.Message);

        var chat = new LessonChat
        {
            LessonId = request.LessonId,
            UserId = userId,
            UserMessage = request.Message,
            AiResponse = aiResponse,
            VideoTimestamp = request.CurrentTimestamp,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.LessonChats.Add(chat);
        await _db.SaveChangesAsync();

        return Ok(chat);
    }

    private string FormatTime(double seconds)
    {
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1 ? t.ToString(@"hh\:mm\:ss") : t.ToString(@"mm\:ss");
    }

    private Dictionary<string, object> TryParseMetadata(string metadata)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(metadata) ?? new();
        } catch
        {
            var dict = new Dictionary<string, object>();
            var clean = metadata.Trim('{', '}', ' ');
            var parts = clean.Split(',');
            foreach(var part in parts)
            {
                var kv = part.Split('=');
                if(kv.Length == 2)
                {
                    dict[kv[0].Trim()] = kv[1].Trim();
                }
            }
            return dict;
        }
    }

    private double ExtractTimestamp(string metadata)
    {
        var meta = TryParseMetadata(metadata);
        if(meta.TryGetValue("Start", out var val))
        {
            if(val is JsonElement je && je.ValueKind == JsonValueKind.Number)
                return je.GetDouble();
            if(double.TryParse(val.ToString(), out var d))
                return d;
        }
        return 0;
    }

    public class ChatRequest
    {
        public int LessonId { get; set; }

        public string Message { get; set; } = string.Empty;

        public double? CurrentTimestamp { get; set; }
    }
}
