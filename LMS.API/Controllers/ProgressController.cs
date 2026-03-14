using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ILeaderboardService _leaderboardService;

    public ProgressController(IProgressService progressService, ILeaderboardService leaderboardService)
    {
        _progressService = progressService;
        _leaderboardService = leaderboardService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("complete")]
    public async Task<ActionResult> CompleteLesson([FromBody] CompleteLessonRequest request)
    {
        try
        {
            var result = await _progressService.CompleteLessonAsync(UserId, request.LessonId);
            await _leaderboardService.CheckAndAwardBadgesAsync(UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("video-position")]
    public async Task<ActionResult> UpdateVideoPosition([FromBody] UpdateVideoPositionRequest request)
        => Ok(await _progressService.UpdateVideoPositionAsync(UserId, request.LessonId, request.PositionSeconds, request.WatchedPercent));

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult> GetCourseProgress(int courseId)
        => Ok(await _progressService.GetCourseProgressAsync(UserId, courseId));

    [HttpGet("my")]
    public async Task<ActionResult> GetMyProgress()
        => Ok(await _progressService.GetUserProgressAsync(UserId));

    [HttpGet("lessons/{courseId}")]
    public async Task<ActionResult> GetLessonProgresses(int courseId)
        => Ok(await _progressService.GetLessonProgressesAsync(UserId, courseId));

    [HttpGet("can-access/{lessonId}")]
    public async Task<ActionResult> CanAccess(int lessonId)
        => Ok(new { canAccess = await _progressService.CanAccessLessonAsync(UserId, lessonId) });
}
