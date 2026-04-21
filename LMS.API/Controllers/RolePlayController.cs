using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolePlayController : ControllerBase
{
    private readonly IRolePlayService _rolePlayService;

    public RolePlayController(IRolePlayService rolePlayService) { _rolePlayService = rolePlayService; }

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }

    [HttpGet("sessions/{lessonId}")]
    public async Task<ActionResult<RolePlaySessionResponse>> GetSession(int lessonId)
    {
        var session = await _rolePlayService.GetSessionAsync(UserId, lessonId);
        if(session == null)
            return NotFound();

        return Ok(
            new RolePlaySessionResponse(
                session.Id,
                session.UserId,
                session.User?.FullName,
                session.LessonId,
                session.Lesson?.Title,
                session.Status,
                session.Score,
                null,
                session.Feedback,
                session.CreatedAt,
                session.Messages.Select(m => new RolePlayMessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToList()));
    }

    [HttpPost("sessions/{lessonId}/start")]
    public async Task<ActionResult<RolePlaySessionResponse>> StartSession(int lessonId)
    {
        var session = await _rolePlayService.StartSessionAsync(UserId, lessonId);
        return Ok(
            new RolePlaySessionResponse(
                session.Id,
                session.UserId,
                session.User?.FullName,
                session.LessonId,
                session.Lesson?.Title,
                session.Status,
                session.Score,
                null,
                session.Feedback,
                session.CreatedAt,
                session.Messages.Select(m => new RolePlayMessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToList()));
    }

    [HttpPost("sessions/{sessionId}/messages")]
    public async Task<ActionResult<RolePlayMessageResponse>> SendMessage(
        int sessionId,
        [FromBody] RolePlaySendMessageRequest request)
    {
        var message = await _rolePlayService.SendMessageAsync(UserId, sessionId, request.Content);
        return Ok(new RolePlayMessageResponse(message.Id, message.Role, message.Content, message.CreatedAt));
    }

    [HttpPost("sessions/{sessionId}/messages/stream")]
    public async Task SendMessageStream(int sessionId, [FromBody] RolePlaySendMessageRequest request)
    {
        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");
        Response.Headers.Append("X-Accel-Buffering", "no");

        await foreach(var chunk in _rolePlayService.SendMessageStreamingAsync(UserId, sessionId, request.Content))
        {
            if(string.IsNullOrEmpty(chunk))
                continue;

            var json = JsonSerializer.Serialize(new { content = chunk });
            await Response.WriteAsync($"data: {json}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    [HttpPost("sessions/{sessionId}/finish")]
    public async Task<ActionResult> FinishSession(int sessionId)
    {
        await _rolePlayService.FinishSessionAsync(UserId, sessionId);
        return Ok();
    }

    [HttpGet("sessions/{lessonId}/history")]
    public async Task<ActionResult<List<RolePlaySessionResponse>>> GetHistory(int lessonId)
    {
        var sessions = await _rolePlayService.GetSessionsByLessonAsync(UserId, lessonId);
        return Ok(
            sessions.Select(
                session => new RolePlaySessionResponse(
                    session.Id,
                    session.UserId,
                    session.User?.FullName,
                    session.LessonId,
                    session.Lesson?.Title,
                    session.Status,
                    session.Score,
                    null,
                    session.Feedback,
                    session.CreatedAt,
                    new List<RolePlayMessageResponse>()))
                .ToList());
    }

    [HttpGet("sessions/details/{sessionId}")]
    public async Task<ActionResult<RolePlaySessionResponse>> GetSessionDetails(int sessionId)
    {
        var session = await _rolePlayService.GetSessionByIdAsync(UserId, sessionId);
        if(session == null)
            return NotFound();

        return Ok(
            new RolePlaySessionResponse(
                session.Id,
                session.UserId,
                session.User?.FullName,
                session.LessonId,
                session.Lesson?.Title,
                session.Status,
                session.Score,
                null,
                session.Feedback,
                session.CreatedAt,
                session.Messages.Select(m => new RolePlayMessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToList()));
    }

    [HttpPost("generate-scenario")]
    public async Task<ActionResult<object>> GenerateScenario([FromBody] List<int> lessonIds)
    {
        var scenario = await _rolePlayService.GenerateScenarioFromLessonsAsync(lessonIds);
        return Ok(new { scenario });
    }
}
