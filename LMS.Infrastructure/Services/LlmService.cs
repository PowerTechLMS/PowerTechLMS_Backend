using System.Text;
using System.Text.Json;
using LMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LMS.Infrastructure.Services;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public LlmService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["LlmSettings:ApiUrl"] ?? throw new ArgumentNullException("LlmSettings:ApiUrl is not configured.");
        _apiKey = configuration["LlmSettings:ApiKey"] ?? string.Empty;
    }

    public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
    {
        var requestBody = new
        {
            model = "gemini-3-flash", 
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature = 0.7
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl.TrimEnd('/')}/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return content ?? string.Empty;
    }

    public async Task<LlmChatResponse> ChatWithToolsAsync(List<LlmChatMessage> messages, List<LlmTool>? tools = null)
    {
        var requestBody = new
        {
            model = "gemini-3-flash",
            messages = messages.Select(m => new
            {
                role = m.Role,
                content = m.Content,
                tool_calls = m.ToolCalls?.Select(tc => new
                {
                    id = tc.Id,
                    type = "function",
                    function = new { name = tc.Name, arguments = tc.Arguments }
                }),
                tool_call_id = m.ToolCallId
            }).ToList(),
            tools = tools != null && tools.Any() ? tools.Select(t => new
            {
                type = "function",
                function = new
                {
                    name = t.Name,
                    description = t.Description,
                    parameters = t.Parameters
                }
            }).ToList() : null,
            temperature = 0.7
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl.TrimEnd('/')}/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull }), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var choice = doc.RootElement.GetProperty("choices")[0];
        var message = choice.GetProperty("message");

        var result = new LlmChatResponse();
        if (message.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != JsonValueKind.Null)
        {
            result.Message = contentProp.GetString();
        }

        if (message.TryGetProperty("tool_calls", out var toolCallsProp) && toolCallsProp.ValueKind == JsonValueKind.Array)
        {
            result.ToolCalls = new List<LlmToolCall>();
            foreach (var tc in toolCallsProp.EnumerateArray())
            {
                var func = tc.GetProperty("function");
                result.ToolCalls.Add(new LlmToolCall
                {
                    Id = tc.GetProperty("id").GetString() ?? string.Empty,
                    Name = func.GetProperty("name").GetString() ?? string.Empty,
                    Arguments = func.GetProperty("arguments").GetString() ?? "{}"
                });
            }
        }

        return result;
    }
}
