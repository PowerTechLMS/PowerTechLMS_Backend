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

    // Thay thế biến UserId cũ bằng thuộc tính an toàn này:
    private int UserId
    {
        get
        {
            // Tìm claim theo nhiều định dạng khác nhau để chống lỗi Null
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("UserId")
                     ?? User.FindFirst("sub");

            if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }
    [HttpGet("learner")]
    public async Task<IActionResult> GetLearnerDashboard()
    {
        // Sử dụng service để lấy dữ liệu THẬT thay vì mock data
        var dashboardData = await _dashboardService.GetLearnerDashboardAsync(UserId);
        return Ok(dashboardData);
    }
}