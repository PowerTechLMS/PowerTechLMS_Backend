using LMS.Core.Interfaces;
using System.Net.Http.Json;

namespace LMS.Infrastructure.Services;

public class AiAgentClient : IAiAgentClient
{
    private readonly HttpClient _httpClient;
    private readonly IAiSidecarManager _sidecarManager;

    public AiAgentClient(HttpClient httpClient, IAiSidecarManager sidecarManager)
    {
        _httpClient = httpClient;
        _sidecarManager = sidecarManager;
    }

    public async Task<AiAgentResponse> ChatAsync(string message, int adminId, string threadId)
    {
        var sidecarUrl = _sidecarManager.SidecarUrl;
        var response = await _httpClient.PostAsJsonAsync($"{sidecarUrl}/chat", new { message, adminId, threadId });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<AiAgentResponse>() ??
            throw new Exception("Không nhận được phản hồi từ AI Sidecar.");
    }
}
