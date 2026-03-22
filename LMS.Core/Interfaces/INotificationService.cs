using LMS.Core.Entities;

namespace LMS.Core.Interfaces;

public interface INotificationService
{
    Task<Notification> CreateNotificationAsync(
        int userId,
        string title,
        string message,
        string? link = null,
        string? type = null);

    Task<List<Notification>> GetUserNotificationsAsync(int userId, bool onlyUnread = false);

    Task MarkAsReadAsync(int notificationId);

    Task CreateAdminReportReminderAsync();
}
