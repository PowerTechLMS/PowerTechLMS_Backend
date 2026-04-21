using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "RolePlayManage")]
public class AdminRolePlayController : ControllerBase
{
    private readonly IRolePlayService _rolePlayService;

    public AdminRolePlayController(IRolePlayService rolePlayService) { _rolePlayService = rolePlayService; }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<RolePlaySessionResponse>>> GetAllSessions()
    {
        var sessions = await _rolePlayService.GetAllSessionsAsync();
        return Ok(sessions);
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<ActionResult<RolePlaySessionResponse>> GetSession(int sessionId)
    {
        var sessions = await _rolePlayService.GetAllSessionsAsync();
        var session = sessions.FirstOrDefault(s => s.Id == sessionId);
        if(session == null)
            return NotFound();

        return Ok(session);
    }

    [HttpPut("sessions/{sessionId}/status")]
    public async Task<ActionResult> UpdateStatus(int sessionId, [FromBody] AdminUpdateRolePlayStatusRequest request)
    {
        await _rolePlayService.UpdateSessionStatusAsync(sessionId, request.Status, request.Score, request.Feedback);
        return Ok();
    }
}
