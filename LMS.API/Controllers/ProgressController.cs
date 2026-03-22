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

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("sub") ??
                User.FindFirst("id") ??
                User.FindFirst("UserId");
            if(claim == null)
                throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }

    [HttpPost("complete")]
    public async Task<ActionResult> CompleteLesson([FromBody] CompleteLessonRequest request)
    {
        try
        {
            var result = await _progressService.CompleteLessonAsync(UserId, request.LessonId, request.IsQuizPassed);
            await _leaderboardService.CheckAndAwardBadgesAsync(UserId);
            return Ok(result);
        } catch(InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message, type = "InvalidOperation" });
        } catch(Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message, inner = ex.InnerException?.Message });
        }
    }

    [HttpPut("video-position")]
    public async Task<ActionResult> UpdateVideoPosition([FromBody] UpdateVideoPositionRequest request) => Ok(
        await _progressService.UpdateVideoPositionAsync(
            UserId,
            request.LessonId,
            request.PositionSeconds,
            request.WatchedPercent));

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult> GetCourseProgress(int courseId)
    {
        try
        {
            return Ok(await _progressService.GetCourseProgressAsync(UserId, courseId));
        } catch(Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy tiến độ khóa học: " + ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult> GetMyProgress()
    {
        try
        {
            return Ok(await _progressService.GetUserProgressAsync(UserId));
        } catch(Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy lịch sử học tập: " + ex.Message });
        }
    }

    [HttpGet("lessons/{courseId}")]
    public async Task<ActionResult> GetLessonProgresses(int courseId)
    {
        try
        {
            return Ok(await _progressService.GetLessonProgressesAsync(UserId, courseId));
        } catch(Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy tiến độ bài học: " + ex.Message });
        }
    }

    [HttpGet("can-access/{lessonId}")]
    public async Task<ActionResult> CanAccess(int lessonId) => Ok(
        new { canAccess = await _progressService.CanAccessLessonAsync(UserId, lessonId) });
}
