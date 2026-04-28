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

    [HttpDelete("questions/{id}")]
    public async Task<ActionResult> DeleteQuestion(int id)
    {
        await _essayService.DeleteEssayQuestionAsync(id);
        return Ok();
    }

    [HttpGet("lessons/{lessonId}/questions")]
    public async Task<ActionResult<List<EssayQuestionDto>>> GetQuestionsByLesson(int lessonId)
    {
        var questions = await _essayService.GetQuestionsByLessonAsync(lessonId);
        return Ok(questions);
    }
}
