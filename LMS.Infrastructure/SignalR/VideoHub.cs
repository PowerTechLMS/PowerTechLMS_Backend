using Microsoft.AspNetCore.SignalR;

namespace LMS.Infrastructure.SignalR;

public class VideoHub : Hub
{
    public async Task JoinLessonGroup(int lessonId)
    { await Groups.AddToGroupAsync(Context.ConnectionId, $"lesson_{lessonId}"); }

    public async Task LeaveLessonGroup(int lessonId)
    { await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"lesson_{lessonId}"); }
}
