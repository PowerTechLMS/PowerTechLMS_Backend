using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
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

    public LessonChatController(AppDbContext db, VectorDbService vectorDb, ILlmService llm, ILogger<LessonChatController> logger)
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

        // RAG: Search for relevant context
        var searchResults = await _vectorDb.SearchAsync(request.Message, request.LessonId, limit: 5);
        
        // Lấy toàn bộ các đoạn của bài học để tìm các đoạn kế tiếp (context expansion)
        var allSegments = await _vectorDb.GetAllSegmentsAsync(request.LessonId);
        var sortedAllSegments = allSegments
            .Select(s => new { s.Content, Timestamp = ExtractTimestamp(s.Metadata) })
            .OrderBy(s => s.Timestamp)
            .ToList();

        var expandedContexts = new HashSet<string>();
        foreach (var res in searchResults)
        {
            var currentTS = ExtractTimestamp(res.Metadata);
            var index = sortedAllSegments.FindIndex(s => Math.Abs(s.Timestamp - currentTS) < 0.1);
            
            if (index != -1)
            {
                // Thêm đoạn hiện tại và tối đa 2 đoạn kế tiếp
                for (int i = index; i <= index + 2 && i < sortedAllSegments.Count; i++)
                {
                    var seg = sortedAllSegments[i];
                    expandedContexts.Add($"- Nội dung: {seg.Content} (Thời gian: {seg.Timestamp}s)");
                }
            }
        }

        var contextText = string.Join("\n", expandedContexts);

        var systemPrompt = 
            "Bạn là một trợ lý học tập thông minh của hệ thống PowerTech. " +
            "Dưới đây là thông tin liên quan đến bài học được trích xuất từ video hoặc tài liệu. " +
            "Hãy trả lời câu hỏi của người dùng dựa trên thông tin này. " +
            "Nếu thông tin có timestamp (dấu thời gian), hãy định dạng nó theo kiểu [[MM:SS]] hoặc [[HH:MM:SS]] để người dùng có thể click vào. " +
            "Nếu câu hỏi không liên quan đến bài học, hãy trả lời lịch sự rằng bạn chỉ có thể hỗ trợ kiến thức trong bài học này.\n\n" +
            "Dữ liệu bài học:\n" +
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

    private double ExtractTimestamp(string metadata)
    {
        // Metadata format: "{ LessonId = 2, Type = Video, Start = 1189.56 }" or JSON "{ \"LessonId\": 2, ... }"
        try
        {
            if (metadata.Contains("Start"))
            {
                var parts = metadata.Split(',');
                var startPart = parts.FirstOrDefault(p => p.Trim().StartsWith("Start"));
                if (startPart != null)
                {
                    var val = startPart.Split('=')[1].Trim().TrimEnd('}');
                    return double.Parse(val);
                }
            }
        } catch { }
        return 0;
    }

    public class ChatRequest
    {
        public int LessonId { get; set; }
        public string Message { get; set; } = string.Empty;
        public double? CurrentTimestamp { get; set; }
    }
}
