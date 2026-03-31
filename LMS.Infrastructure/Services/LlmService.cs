using LMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LMS.Infrastructure.Services;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;

    public LlmService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiBaseUrl = configuration["LlmSettings:ApiUrl"] ??
            throw new ArgumentNullException("LlmSettings:ApiUrl is not configured.");
        _apiKey = configuration["LlmSettings:ApiKey"] ?? string.Empty;
    }

    public async Task<string> GenerateResponseAsync(string systemPrompt, string userPrompt)
    {
        if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            return "Hệ thống AI chưa được kích hoạt: Bạn cần cấu hình Gemini API Key hợp lệ trong file appsettings.json của Backend.";
        }

        try
        {
            var requestBody = new
            {
                model = "gemini-1.5-flash",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.7
            };

            var endpoint = _apiBaseUrl.Contains("generativelanguage.googleapis.com")
                ? $"{_apiBaseUrl.TrimEnd('/')}/v1beta/openai/v1/chat/completions?key={_apiKey}"
                : $"{_apiBaseUrl.TrimEnd('/')}/v1/chat/completions";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            if (!endpoint.Contains("key="))
            {
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            }
            
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                return $"Lỗi kết nối AI (Status {response.StatusCode}): {errorMsg}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var content = doc.RootElement.GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return content ?? "AI không trả về kết quả.";
        }
        catch (Exception ex)
        {
            return $"Lỗi xử lý AI: {ex.Message}";
        }
    }

    public async Task<LlmChatResponse> ChatWithToolsAsync(List<LlmChatMessage> messages, List<LlmTool>? tools = null)
    {
        var requestBody = new
        {
            model = "gemini-1.5-flash",
            messages = messages.Select(
                m => new
                {
                    role = m.Role,
                    content = m.Content,
                    tool_calls = m.ToolCalls?.Select(
                        tc => new
                            {
                                id = tc.Id,
                                type = "function",
                                function = new { name = tc.Name, arguments = tc.Arguments }
                            }),
                    tool_call_id = m.ToolCallId
                })
                .ToList(),
            tools = tools != null && tools.Any()
                ? tools.Select(
                    t => new
                    {
                        type = "function",
                        function = new { name = t.Name, description = t.Description, parameters = t.Parameters }
                    })
                    .ToList()
                : null,
            temperature = 0.7
        };

        var endpoint = _apiBaseUrl.Contains("generativelanguage.googleapis.com") && !_apiBaseUrl.Contains("v1beta/openai")
            ? $"{_apiBaseUrl.TrimEnd('/')}/v1beta/openai/v1/chat/completions"
            : $"{_apiBaseUrl.TrimEnd('/')}/v1/chat/completions";

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");
        request.Content = new StringContent(
            JsonSerializer.Serialize(
                requestBody,
                new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseString);
        var choice = doc.RootElement.GetProperty("choices")[0];
        var message = choice.GetProperty("message");

        var result = new LlmChatResponse();
        if(message.TryGetProperty("content", out var contentProp) && contentProp.ValueKind != JsonValueKind.Null)
        {
            result.Message = contentProp.GetString();
        }

        if(message.TryGetProperty("tool_calls", out var toolCallsProp) && toolCallsProp.ValueKind == JsonValueKind.Array)
        {
            result.ToolCalls = new List<LlmToolCall>();
            foreach(var tc in toolCallsProp.EnumerateArray())
            {
                var func = tc.GetProperty("function");
                result.ToolCalls
                    .Add(
                        new LlmToolCall
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
