using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Policy = "EssayManage")]
public class AdminEssayController : ControllerBase
{
    private readonly IEssayService _essayService;

    public AdminEssayController(IEssayService essayService) { _essayService = essayService; }

    [HttpGet("attempts")]
    public async Task<ActionResult<List<AdminEssayAttemptResponse>>> GetAllAttempts()
    {
        var attempts = await _essayService.GetAllAttemptsAsync();
        return Ok(attempts);
    }

    [HttpGet("attempts/{id}")]
    public async Task<ActionResult<EssayResultResponse>> GetAttemptDetail(int id)
    {
        var detail = await _essayService.GetAttemptDetailAsync(0, id);
        return Ok(detail);
    }

    [HttpPut("attempts/{id}")]
    public async Task<ActionResult> UpdateAttempt(int id, [FromBody] AdminUpdateEssayAttemptRequest request)
    {
        await _essayService.UpdateAttemptAsync(id, request);
        return Ok();
    }
}
