using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiSuggestionController : ControllerBase
{
    private readonly ILlmService _llm;
    private readonly AppDbContext _db;

    public AiSuggestionController(ILlmService llm, AppDbContext db)
    {
        _llm = llm;
        _db = db;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        if (request.Messages == null || !request.Messages.Any())
            return BadRequest(new { message = "Tin nhắn không được để trống." });

        var tools = new List<LlmTool>
        {
            new LlmTool
            {
                Name = "search_courses",
                Description = "Tìm kiếm các khóa học tự do (Level 3) trong hệ thống dựa trên từ khóa tiêu đề hoặc mô tả.",
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        query = new { type = "string", description = "Từ khóa tìm kiếm (ví dụ: 'javascript', 'kỹ năng mềm', 'dự án')" }
                    },
                    required = new[] { "query" }
                }
            }
        };

        var systemPrompt = "Bạn là trợ lý đào tạo thông minh của hệ thống LMS. Nhiệm vụ của bạn là giúp người dùng tìm kiếm khóa học phù hợp. " +
                           "Hãy sử dụng công cụ search_courses để tìm kiếm các khóa học Level 3 dựa trên nhu cầu của họ. " +
                           "Sau khi có kết quả từ công cụ, hãy giới thiệu các khóa học đó một cách hấp dẫn. " +
                           "Nếu kết quả tìm kiếm trống hoặc không có khóa học nào thực sự phù hợp, tuyệt đối KHÔNG tự bịa ra khóa học. " +
                           "Thay vào đó, hãy trả lời: 'Rất tiếc, hiện tại hệ thống chưa có khóa học chính xác theo yêu cầu của bạn. Bạn vui lòng gửi yêu cầu đề xuất thêm khóa học mới lên Trưởng phòng hoặc Cấp trên của mình để được hỗ trợ nhé'. " +
                           "Hãy luôn trả lời bằng tiếng Việt thân thiện, lịch sự.";

        var chatHistory = request.Messages;
        if (!chatHistory.Any(m => m.Role == "system"))
        {
            chatHistory.Insert(0, new LlmChatMessage { Role = "system", Content = systemPrompt });
        }

        try
        {
            var response = await _llm.ChatWithToolsAsync(chatHistory, tools);

            // Xử lý tối đa 2 vòng gọi tool để tránh lặp vô tận (thực tế 1 vòng là đủ cho search)
            int loopCount = 0;
            while (response.ToolCalls != null && response.ToolCalls.Any() && loopCount < 2)
            {
                chatHistory.Add(new LlmChatMessage 
                { 
                    Role = "assistant", 
                    Content = response.Message, 
                    ToolCalls = response.ToolCalls 
                });

                foreach (var tc in response.ToolCalls)
                {
                    if (tc.Name == "search_courses")
                    {
                        var args = JsonSerializer.Deserialize<JsonElement>(tc.Arguments);
                        var query = args.TryGetProperty("query", out var q) ? q.GetString() : "";
                        
                        var matches = await _db.Courses
                            .Where(c => c.Level == 3 && c.IsPublished && !c.IsDeleted &&
                                        (EF.Functions.ILike(c.Title, $"%{query}%") || EF.Functions.ILike(c.Description, $"%{query}%")))
                            .Take(5)
                            .Select(c => new { c.Id, c.Title, c.Description })
                            .ToListAsync();

                        chatHistory.Add(new LlmChatMessage 
                        { 
                            Role = "tool", 
                            ToolCallId = tc.Id, 
                            Content = JsonSerializer.Serialize(matches) 
                        });
                    }
                }

                response = await _llm.ChatWithToolsAsync(chatHistory, tools);
                loopCount++;
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi xử lý AI: " + ex.Message });
        }
    }
}

public class ChatRequest
{
    public List<LlmChatMessage> Messages { get; set; } = new();
}
