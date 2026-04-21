using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EssayController : ControllerBase
{
    private readonly IEssayService _essayService;

    public EssayController(IEssayService essayService) { _essayService = essayService; }

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }

    private bool IsAdmin => User.IsInRole("Admin") ||
        User.IsInRole("Quản trị viên") ||
        User.HasClaim("permission", "user.manage");

    [HttpPost("lessons/{lessonId}/start")]
    public async Task<ActionResult<StartEssayAttemptResponse>> StartAttempt(int lessonId)
    {
        var attempt = await _essayService.StartAttemptAsync(UserId, lessonId);
        var detail = await _essayService.GetAttemptDetailAsync(UserId, attempt.Id);
        var attemptNumber = await _essayService.GetAttemptNumberAsync(UserId, lessonId, attempt.StartedAt);

        return Ok(
            new StartEssayAttemptResponse(
                attempt.Id,
                attemptNumber,
                attempt.StartedAt,
                detail.Answers.FirstOrDefault()?.Weight,
                detail.Answers
                    .Select(
                        a => new EssayQuestionItemResponse(
                                    a.QuestionId,
                                    a.QuestionContent,
                                    0,
                                    a.Weight,
                                    a.ScoringCriteria))
                    .ToList(),
                0,
                null));
    }

    [HttpPost("attempts/{attemptId}/submit")]
    public async Task<ActionResult<EssayResultResponse>> SubmitAttempt(
        int attemptId,
        [FromBody] SubmitEssayRequest request)
    {
        var result = await _essayService.SubmitAttemptAsync(UserId, attemptId, request);
        return Ok(result);
    }

    [HttpGet("lessons/{lessonId}/history")]
    public async Task<ActionResult<List<EssayAttemptSummary>>> GetHistory(int lessonId)
    {
        var history = await _essayService.GetAttemptsByLessonAsync(UserId, lessonId, IsAdmin);
        return Ok(history);
    }

    [HttpGet("attempts/{attemptId}")]
    public async Task<ActionResult<EssayResultResponse>> GetAttemptDetail(int attemptId)
    {
        var detail = await _essayService.GetAttemptDetailAsync(UserId, attemptId, IsAdmin);
        return Ok(detail);
    }

    [HttpGet("lessons/{lessonId}/active")]
    public async Task<ActionResult<StartEssayAttemptResponse?>> GetActiveAttempt(int lessonId)
    {
        var attempt = await _essayService.GetActiveAttemptAsync(UserId, lessonId);
        if(attempt is null)
            return Ok(null);

        var attemptNumber = await _essayService.GetAttemptNumberAsync(UserId, lessonId, attempt.StartedAt);

        return Ok(
            new StartEssayAttemptResponse(
                attempt.Id,
                attemptNumber,
                attempt.StartedAt,
                attempt.Lesson.EssayConfig?.TimeLimitMinutes,
                attempt.Lesson.EssayConfig?.Questions.Select(
                        q => new EssayQuestionItemResponse(q.Id, q.Content, q.SortOrder, q.Weight, q.ScoringCriteria))
                        .OrderBy(q => q.SortOrder)
                        .ToList() ??
                    new List<EssayQuestionItemResponse>(),
                0,
                attempt.Answers.Select(a => new EssayAnswerSubmit(a.QuestionId, a.Content)).ToList()));
    }

    [HttpPost("attempts/{attemptId}/save-draft")]
    public async Task<IActionResult> SaveDraft(int attemptId, [FromBody] SubmitEssayRequest request)
    {
        await _essayService.SaveDraftAsync(UserId, attemptId, request);
        return Ok();
    }

    [HttpPost("generate-questions")]
    public async Task<ActionResult<List<EssayQuestionDto>>> GenerateQuestions([FromBody] List<int> lessonIds)
    {
        var questions = await _essayService.GenerateQuestionsFromLessonsAsync(lessonIds);
        return Ok(questions);
    }
}
