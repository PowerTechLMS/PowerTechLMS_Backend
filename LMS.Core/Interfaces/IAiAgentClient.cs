using System.Text.Json.Serialization;

namespace LMS.Core.Interfaces;

public interface IAiAgentClient
{
    Task<AiAgentResponse> ChatAsync(string message, int adminId, string threadId);
}

public record AiAgentResponse(
    [property: JsonPropertyName("response")] string Response,
    [property: JsonPropertyName("status")] string Status = "success"
);
