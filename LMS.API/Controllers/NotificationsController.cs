using LMS.Core.Entities;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    public NotificationsController(INotificationService notificationService)
    { _notificationService = notificationService; }

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

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Notification>>> Get([FromQuery] bool onlyUnread = false)
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(UserId, onlyUnread);
        return Ok(notifications);
    }

    [HttpPost]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult<Notification>> Create([FromBody] CreateNotificationDto dto)
    {
        var notification = await _notificationService.CreateNotificationAsync(
            dto.UserId,
            dto.Title,
            dto.Message,
            dto.Link,
            dto.Type);
        return CreatedAtAction(nameof(Get), new { }, notification);
    }

    [HttpPost("{id}/read")]
    [Authorize]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        await _notificationService.MarkAsReadAsync(id);
        return NoContent();
    }

    [HttpPost("remind-admin-report")]
    [Authorize(Policy = "UserManage")]
    public async Task<IActionResult> RemindAdminReport()
    {
        await _notificationService.CreateAdminReportReminderAsync();
        return Ok();
    }
}

public record CreateNotificationDto(int UserId, string Title, string Message, string? Link = null, string? Type = null);
