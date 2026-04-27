using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/admin-ai")]
[Authorize]
public class AdminAiController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAiToolService _toolService;
    private readonly IAiAgentClient _aiAgentClient;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly IHubContext<AiHub> _hubContext;

    public AdminAiController(
        AppDbContext db,
        IAiToolService toolService,
        IAiAgentClient aiAgentClient,
        IConfiguration config,
        IEmailService emailService,
        IHubContext<AiHub> hubContext)
    {
        _db = db;
        _toolService = toolService;
        _aiAgentClient = aiAgentClient;
        _config = config;
        _emailService = emailService;
        _hubContext = hubContext;
    }

    [HttpPost("sessions/{id}/chat")]
    public async Task<IActionResult> Chat(int id, [FromBody] AdminAiChatRequest request)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _db.AdminAiSessions.FirstOrDefaultAsync(s => s.Id == id && s.CreatedById == adminId);
        if(session is null)
        {
            return NotFound(new { message = "Không tìm thấy phiên chat." });
        }

        var userMsg = new AdminAiMessage
        {
            SessionId = id,
            Role = "user",
            Content = request.Message,
            CreatedAt = DateTime.UtcNow
        };
        _db.AdminAiMessages.Add(userMsg);
        await _db.SaveChangesAsync();

        try
        {
            var aiResponse = await _aiAgentClient.ChatAsync(request.Message, adminId, session.ThreadId!);

            var aiMsg = new AdminAiMessage
            {
                SessionId = id,
                Role = "assistant",
                Content = aiResponse.Response,
                CreatedAt = DateTime.UtcNow
            };
            _db.AdminAiMessages.Add(aiMsg);
            await _db.SaveChangesAsync();

            return Ok(aiMsg);
        } catch(Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gọi AI Sidecar.", detail = ex.Message });
        }
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var sessions = await _db.AdminAiSessions
            .Where(s => s.CreatedById == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new { s.Id, s.Title, s.CreatedAt, s.ThreadId, s.LastProgressJson })
            .ToListAsync();
        return Ok(sessions);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateAiSessionRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = new AdminAiSession
        {
            Title = request.Title,
            CreatedById = userId,
            ThreadId = Guid.NewGuid().ToString()
        };
        _db.AdminAiSessions.Add(session);
        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpPut("sessions/{id}")]
    public async Task<IActionResult> UpdateSession(int id, [FromBody] UpdateAiSessionRequest request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _db.AdminAiSessions.FirstOrDefaultAsync(s => s.Id == id && s.CreatedById == userId);
        if(session is null)
        {
            return NotFound(new { message = "Không tìm thấy phiên chat." });
        }

        session.Title = request.Title;
        await _db.SaveChangesAsync();
        return Ok(session);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _db.AdminAiSessions
            .Include(s => s.Messages)
            .Include(s => s.Tasks)
            .FirstOrDefaultAsync(s => s.Id == id && s.CreatedById == userId);

        if(session is null)
        {
            return NotFound(new { message = "Không tìm thấy phiên chat." });
        }

        _db.AdminAiMessages.RemoveRange(session.Messages);
        _db.AiTasks.RemoveRange(session.Tasks);
        _db.AdminAiSessions.Remove(session);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Đã xóa phiên chat thành công." });
    }

    [HttpGet("sessions/{id}/messages")]
    public async Task<IActionResult> GetMessages(int id)
    {
        var messages = await _db.AdminAiMessages.Where(m => m.SessionId == id).OrderBy(m => m.CreatedAt).ToListAsync();
        return Ok(messages);
    }

    [HttpPost("execute-tool")]
    [AllowAnonymous]
    public async Task<IActionResult> ExecuteTool([FromBody] ExecuteToolRequest request)
    {
        var secret = Request.Headers["X-Internal-Secret"].ToString();
        var expectedSecret = _config["Jwt:Secret"];

        if(string.IsNullOrEmpty(secret) || secret != expectedSecret)
        {
            if(!User.Identity?.IsAuthenticated ?? true)
            {
                return Unauthorized();
            }
        }

        if(request.ToolName == "NotifyProgress")
        {
            var update = JsonSerializer.Deserialize<AiProgressUpdate>(
                request.ArgumentsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if(update is not null)
            {
                await NotifyProgress(update);
                return Ok(new AiToolResponse(true, "Progress verified"));
            }
        }

        var result = await _toolService.ExecuteToolAsync(request.ToolName, request.ArgumentsJson, request.AdminId);
        return Ok(result);
    }

    [HttpPost("notify-progress")]
    [AllowAnonymous]
    public async Task<IActionResult> NotifyProgress([FromBody] AiProgressUpdate update)
    {
        await _hubContext.Clients.Group($"thread_{update.ThreadId}").SendAsync("OnAiProgress", update);

        var session = await _db.AdminAiSessions.FirstOrDefaultAsync(s => s.ThreadId == update.ThreadId);
        if(session is not null)
        {
            session.LastProgressJson = JsonSerializer.Serialize(update);

            var task = await _db.AiTasks
                .FirstOrDefaultAsync(
                    t => t.SessionId == session.Id && t.Topic == update.Step && t.JobId == update.ThreadId);

            if(task is null && update.Status != "planned")
            {
                task = await _db.AiTasks
                    .Where(
                        t => t.SessionId == session.Id &&
                            (t.Status == "planned" || t.Status == "Đang chờ phê duyệt") &&
                            !t.IsCompleted)
                    .OrderBy(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                if(task is not null)
                {
                    task.Topic = update.Step;
                }
            }

            if(task is null &&
                (update.Status == "running" || update.Status == "completed" || update.Status == "planned"))
            {
                task = new AiTask
                {
                    SessionId = session.Id,
                    Topic = update.Step,
                    JobId = update.ThreadId,
                    Progress = update.Progress,
                    Status = update.Detail ?? update.Status,
                    CreatedById = session.CreatedById,
                    CreatedAt = DateTime.UtcNow
                };
                _db.AiTasks.Add(task);
            }

            if(task is not null)
            {
                task.Progress = update.Progress;
                task.Status = update.Detail ?? update.Status;
                task.IsCompleted = update.Status == "completed";
                task.IsFailed = update.Status == "error";
                if(task.IsFailed)
                    task.ErrorMessage = update.Detail;
            }

            await _db.SaveChangesAsync();
        }

        return Ok();
    }

    [HttpGet("sessions/{id}/tasks")]
    public async Task<IActionResult> GetTasks(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var session = await _db.AdminAiSessions.AnyAsync(s => s.Id == id && s.CreatedById == userId);
        if(!session)
            return NotFound();

        var tasks = await _db.AiTasks
            .Where(t => t.SessionId == id)
            .OrderBy(t => t.CreatedAt)
            .Select(t => new { t.Id, t.Topic, t.Progress, t.Status, t.IsCompleted, t.IsFailed, t.CreatedAt })
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpGet("available-tools")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAvailableTools([FromQuery] int? adminId, [FromQuery] string? query)
    {
        var secret = Request.Headers["X-Internal-Secret"].ToString();
        var expectedSecret = _config["Jwt:Secret"];

        int targetAdminId = 0;
        if(!string.IsNullOrEmpty(secret) && secret == expectedSecret)
        {
            if(!adminId.HasValue)
                return BadRequest("Internal call requires adminId.");
            targetAdminId = adminId.Value;
        } else
        {
            if(!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();
            targetAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        var tools = await _toolService.GetAvailableToolsAsync(targetAdminId, query);
        return Ok(tools);
    }

    [HttpDelete("tasks/{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var task = await _db.AiTasks
            .Include(t => t.Session)
            .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedById == userId);

        if(task is null)
            return NotFound();
        if(task.IsCompleted)
            return BadRequest(new { message = "Không thể xóa tác vụ đã hoàn thành." });

        _db.AiTasks.Remove(task);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Đã xóa tác vụ khỏi kế hoạch." });
    }

    [HttpPost("send-report")]
    [AllowAnonymous]
    public async Task<IActionResult> SendReport([FromBody] SendEmailReportRequest request)
    {
        var secret = Request.Headers["X-Internal-Secret"].ToString();
        var expectedSecret = _config["Jwt:Secret"];
        if(string.IsNullOrEmpty(secret) || secret != expectedSecret)
        {
            return Unauthorized();
        }

        string targetEmail = string.Empty;

        if(!string.IsNullOrWhiteSpace(request.ToEmail))
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == request.ToEmail);
            if(!exists)
            {
                return BadRequest(
                    new
                    {
                        message = $"Email '{request.ToEmail}' không tồn tại trong hệ thống. Chỉ được phép gửi báo cáo cho người dùng trong hệ thống."
                    });
            }
            targetEmail = request.ToEmail;
        } else
        {
            var admin = await _db.Users.FindAsync(request.AdminId);
            if(admin is null || string.IsNullOrEmpty(admin.Email))
            {
                return BadRequest(new { message = "Không tìm thấy thông tin Admin người gửi." });
            }
            targetEmail = admin.Email;
        }

        await _emailService.SendEmailAsync(targetEmail, request.Subject, request.Body);
        return Ok(new { message = $"Đã gửi báo cáo thành công tới {targetEmail}." });
    }

    [HttpGet("infographics")]
    public async Task<IActionResult> GetInfographics([FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var query = _db.LessonInfographics.Include(li => li.Lessons).OrderByDescending(li => li.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(
            new
            {
                total,
                items = items.Select(
                    li => new
                        {
                            li.Id,
                            li.ImageUrl,
                            li.Summary,
                            li.CreatedAt,
                            Lessons = li.Lessons.Select(l => new { l.Id, l.Title, l.Type })
                        })
            });
    }

    [HttpDelete("infographics/{id}")]
    public async Task<IActionResult> DeleteInfographic(int id)
    {
        var infographic = await _db.LessonInfographics.FindAsync(id);
        if(infographic is null)
            return NotFound();

        _db.LessonInfographics.Remove(infographic);
        await _db.SaveChangesAsync();

        try
        {
            var storageRoot = _config["Storage:RootPath"];
            var rootPath = string.IsNullOrEmpty(storageRoot)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : storageRoot;

            var filePath = Path.Combine(rootPath, infographic.ImageUrl.TrimStart('/'));
            if(System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        } catch
        {
        }

        return Ok(new { message = "Đã xóa Infographic thành công." });
    }
}

public record CreateAiSessionRequest(string Title);

public record UpdateAiSessionRequest(string Title);

public record ExecuteToolRequest(string ToolName, string ArgumentsJson, int AdminId);

public record AdminAiChatRequest(string Message);

public record AiProgressUpdate(string ThreadId, string Step, string Status, int Progress, string? Detail = null);

public record SendEmailReportRequest(int AdminId, string Subject, string Body, string? ToEmail = null);
