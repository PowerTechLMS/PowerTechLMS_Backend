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
}
