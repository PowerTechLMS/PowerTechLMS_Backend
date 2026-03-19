using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ReportView")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService) => _reportService = reportService;

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
    private bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Quản trị viên") || User.HasClaim("permission", "user.manage");

    [HttpGet("training")]
    public async Task<ActionResult> GetTrainingReport([FromQuery] int? courseId)
        => Ok(await _reportService.GetTrainingReportAsync(courseId, UserId, IsAdmin));

    [HttpGet("inactive")]
    public async Task<ActionResult> GetInactive([FromQuery] int days = 30)
        => Ok(await _reportService.GetInactiveUsersAsync(days, UserId, IsAdmin));

    [HttpGet("quiz-analytics/{quizId}")]
    public async Task<ActionResult> GetQuizAnalytics(int quizId)
    {
        try { return Ok(await _reportService.GetQuizAnalyticsAsync(quizId, UserId, IsAdmin)); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }
}
