using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _service;
    public LeaderboardController(ILeaderboardService service) => _service = service;

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("UserId")
                     ?? User.FindFirst("sub");

            if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }
    [HttpGet]
    public async Task<ActionResult> GetLeaderboard([FromQuery] int top = 10)
        => Ok(await _service.GetLeaderboardAsync(top));


    [HttpGet("monthly")]
    public async Task<ActionResult> GetMonthlyLeaderboard() => Ok(await _service.GetMonthlyLeaderboardAsync());

    [HttpGet("badges/{userId}")]
    public async Task<ActionResult> GetBadges(int userId) => Ok(await _service.GetUserBadgesAsync(userId));
    [HttpGet("badges")]
    public IActionResult GetMyBadges()
    {
        return Ok(new[] {
            new { BadgeId = 1, BadgeName = "Chăm chỉ", IsEarned = true },
            new { BadgeId = 2, BadgeName = "Điểm tuyệt đối", IsEarned = true }
        });
    }

}
