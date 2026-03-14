using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;
    public QuizzesController(IQuizService quizService) => _quizService = quizService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetQuizDetail(int id)
    {
        try
        {
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null) return NotFound(new { message = "Không tìm thấy bài thi" });

            return Ok(quiz);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi Backend", error = ex.Message });
        }
    }

    [HttpPost("course/{courseId}")]
    [Authorize(Policy = "QuizCreate")]
    public async Task<ActionResult> CreateQuiz(int courseId, [FromBody] CreateQuizRequest request)
        => Ok(await _quizService.CreateQuizAsync(courseId, request));

    [HttpPost("{quizId}/questions")]
    [Authorize(Policy = "QuizCreate")]
    public async Task<ActionResult> AddQuestion(int quizId, [FromBody] CreateQuestionRequest request)
    {
        try
        {
            var result = await _quizService.AddQuestionAsync(quizId, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpPost("{quizId}/start")]
    public async Task<ActionResult> StartQuiz(int quizId)
    {
        try { return Ok(await _quizService.StartQuizAsync(UserId, quizId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{attemptId}/draft")]
    public async Task<ActionResult> SaveDraft(int attemptId, [FromBody] SaveDraftRequest request)
    {
        try { await _quizService.SaveAnswerDraftAsync(attemptId, UserId, request.QuestionId, request.SelectedAnswer); return Ok(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{attemptId}/time")]
    public async Task<ActionResult> UpdateTime(int attemptId, [FromBody] UpdateTimeRequest request)
    {
        await _quizService.UpdateRemainingTimeAsync(attemptId, UserId, request.RemainingSeconds);
        return Ok();
    }

    [HttpPost("{attemptId}/submit")]
    public async Task<ActionResult> SubmitQuiz(int attemptId, [FromBody] SubmitQuizRequest request)
    {
        try { return Ok(await _quizService.SubmitQuizAsync(UserId, attemptId, request)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{quizId}/results")]
    public async Task<ActionResult> GetResults(int quizId)
        => Ok(await _quizService.GetUserQuizResultsAsync(UserId, quizId));
}
