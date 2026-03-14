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

    [HttpGet("training")]
    public async Task<ActionResult> GetTrainingReport([FromQuery] int? courseId)
        => Ok(await _reportService.GetTrainingReportAsync(courseId));

    [HttpGet("inactive")]
    public async Task<ActionResult> GetInactive([FromQuery] int days = 30)
        => Ok(await _reportService.GetInactiveUsersAsync(days));

    [HttpGet("quiz-analytics/{quizId}")]
    public async Task<ActionResult> GetQuizAnalytics(int quizId)
        => Ok(await _reportService.GetQuizAnalyticsAsync(quizId));
}
