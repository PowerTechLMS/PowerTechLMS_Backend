using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DocumentChatController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly VectorDbService _vectorDb;
    private readonly ILlmService _llm;

    public DocumentChatController(AppDbContext db, VectorDbService vectorDb, ILlmService llm)
    {
        _db = db;
        _vectorDb = vectorDb;
        _llm = llm;
    }

    [HttpGet("{documentId}")]
    public async Task<IActionResult> GetChatHistory(int documentId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var history = await _db.DocumentChats
            .Where(c => c.DocumentId == documentId && c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return Ok(history);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] DocumentChatRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var document = await _db.Documents.FindAsync(request.DocumentId);
        if(document is null)
            return NotFound("Không tìm thấy tài liệu.");

        var searchResults = await _vectorDb.SearchByDocumentAsync(request.Message, request.DocumentId, limit: 5);

        var contextText = string.Join("\n", searchResults.Select(r => $"- {r.Content}"));

        var systemPrompt = 
            "Bạn là một trợ lý thông minh hỗ trợ giải đáp thắc mắc về tài liệu trong hệ thống PowerTech. " +
            "Dưới đây là nội dung liên quan trích xuất từ tài liệu này. " +
            "Hãy trả lời câu hỏi của người dùng một cách chính xác và chuyên nghiệp dựa trên nội dung được cung cấp. " +
            "Nếu nội dung không cung cấp đủ thông tin để trả lời, hãy nói rằng bạn không tìm thấy thông tin này trong tài liệu.\n\n" +
            "Nội dung tài liệu:\n" +
            contextText;

        var aiResponse = await _llm.GenerateResponseAsync(systemPrompt, request.Message);

        var chat = new DocumentChat
        {
            DocumentId = request.DocumentId,
            UserId = userId,
            UserMessage = request.Message,
            AiResponse = aiResponse,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.DocumentChats.Add(chat);
        await _db.SaveChangesAsync();

        return Ok(chat);
    }

    public class DocumentChatRequest
    {
        public int DocumentId { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
