using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("id") ??
                User.FindFirst("UserId") ??
                User.FindFirst("sub");

            if(claim == null)
                throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }

    [HttpGet("learner")]
    public async Task<IActionResult> GetLearnerDashboard()
    {
        var dashboardData = await _dashboardService.GetLearnerDashboardAsync(UserId);
        return Ok(dashboardData);
    }
}