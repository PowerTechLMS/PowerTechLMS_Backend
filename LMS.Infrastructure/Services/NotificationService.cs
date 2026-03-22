using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(AppDbContext db, IHubContext<NotificationHub> hubContext)
    {
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<Notification> CreateNotificationAsync(
        int userId,
        string title,
        string message,
        string? link = null,
        string? type = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Link = link,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);

        return notification;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool onlyUnread = false)
    {
        IQueryable<Notification> query = _db.Notifications.Where(n => n.UserId == userId);

        if(onlyUnread)
            query = query.Where(n => !n.IsRead);

        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        var notification = await _db.Notifications.FindAsync(notificationId);
        if(notification == null)
            return;
        notification.IsRead = true;
        notification.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task CreateAdminReportReminderAsync()
    {
        var admins = await _db.Users.Where(u => u.Role == "Admin").ToListAsync();
        foreach(var admin in admins)
        {
            await CreateNotificationAsync(
                admin.Id,
                "Nhắc nhở xem báo cáo",
                "Đã đến lúc xem báo cáo hoạt động của hệ thống.",
                "/admin/reports",
                "Reminder");
        }
    }
}
