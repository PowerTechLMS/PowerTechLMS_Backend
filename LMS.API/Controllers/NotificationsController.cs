using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Core.Interfaces;
using LMS.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("UserId")
                     ?? User.FindFirst("sub");

            if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }

    // Get notifications for current user (userId from JWT claim "sub" or "userId")
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Notification>>> Get([FromQuery] bool onlyUnread = false)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(UserId, onlyUnread);
        return Ok(notifications);
    }

    // Create a notification (admin only)
    [HttpPost]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult<Notification>> Create([FromBody] CreateNotificationDto dto)
    {
        var notification = await _notificationService.CreateNotificationAsync(dto.UserId, dto.Title, dto.Message, dto.Link, dto.Type);
        return CreatedAtAction(nameof(Get), new { }, notification);
    }

    // Mark as read
    [HttpPost("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    // Trigger admin report reminder (admin only)
    [HttpPost("remind-admin-report")]
    [Authorize(Policy = "UserManage")]
    public async Task<IActionResult> RemindAdminReport()
    {
        await _notificationService.CreateAdminReportReminderAsync();
        return Ok();
    }
}

public record CreateNotificationDto(int UserId, string Title, string Message, string? Link = null, string? Type = null);
