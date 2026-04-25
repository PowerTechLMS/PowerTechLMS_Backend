using Microsoft.AspNetCore.SignalR;

namespace LMS.Infrastructure.SignalR;

public class AiHub : Hub
{
    public async Task JoinJobGroup(string jobId) { await Groups.AddToGroupAsync(Context.ConnectionId, $"job_{jobId}"); }

    public async Task JoinThread(string threadId)
    { await Groups.AddToGroupAsync(Context.ConnectionId, $"thread_{threadId}"); }
}
