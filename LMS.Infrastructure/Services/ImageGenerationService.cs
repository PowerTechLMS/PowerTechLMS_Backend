using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public interface IImageGenerationService
{
    Task<string> GenerateImageAsync(string prompt);
}

public class ImageGenerationService : IImageGenerationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _apiBaseUrl;
    private readonly string _apiKey;
    private const string ImageModel = "gemini-3.1-flash-image";

    public ImageGenerationService(
        HttpClient httpClient,
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
        _apiBaseUrl = configuration["LlmSettings:ApiUrl"] ??
            throw new ArgumentNullException("LlmSettings:ApiUrl is not configured.");
        _apiKey = configuration["LlmSettings:ApiKey"] ?? string.Empty;
    }

    public async Task<string> GenerateImageAsync(string prompt)
    {
        if(string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("API Key chưa được cấu hình.");
        }

        try
        {
            var requestBody = new { model = ImageModel, prompt = prompt, n = 1, size = "1024x1024" };

            var endpoint = _apiBaseUrl.Contains("generativelanguage.googleapis.com")
                ? $"{_apiBaseUrl.TrimEnd('/')}/v1beta/openai/v1/images/generations"
                : $"{_apiBaseUrl.TrimEnd('/')}/v1/images/generations";

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(request);

            if(!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi tạo ảnh (Status {response.StatusCode}): {errorMsg}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var dataElement = doc.RootElement.GetProperty("data")[0];

            string base64Data = string.Empty;
            if(dataElement.TryGetProperty("b64_json", out var b64Prop))
            {
                base64Data = b64Prop.GetString() ?? string.Empty;
            } else if(dataElement.TryGetProperty("url", out var urlProp))
            {
                return urlProp.GetString() ?? throw new Exception("Không nhận được URL ảnh từ AI.");
            }

            if(string.IsNullOrEmpty(base64Data))
            {
                throw new Exception("Không nhận được dữ liệu ảnh từ AI (Base64).");
            }

            var storageRoot = _configuration["Storage:RootPath"];
            var rootPath = string.IsNullOrEmpty(storageRoot)
                ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                : storageRoot;

            var uploadsFolder = Path.Combine(rootPath, "uploads", "infographics");
            if(!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(uploadsFolder, fileName);
            var imageBytes = Convert.FromBase64String(base64Data);
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return $"/uploads/infographics/{fileName}";
        } catch(Exception ex)
        {
            throw new Exception($"Lỗi xử lý sinh ảnh: {ex.Message}");
        }
    }
}
