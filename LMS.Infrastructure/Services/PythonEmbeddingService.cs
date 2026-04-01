using LMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class PythonEmbeddingService : IPythonEmbeddingService
{
    private readonly IPythonEnvService _pythonEnv;
    private readonly ILogger<PythonEmbeddingService> _logger;
    private readonly string _scriptPath;
    private readonly string _modelName;

    private readonly string _qdrantUrl;

    public PythonEmbeddingService(IPythonEnvService pythonEnv, IConfiguration configuration, ILogger<PythonEmbeddingService> logger)
    {
        _pythonEnv = pythonEnv;
        _logger = logger;
        _qdrantUrl = configuration["Qdrant:Url"] ?? "http://localhost:6334";
        
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, "AI", "embed.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "AI", "embed.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "LMS.Infrastructure", "AI", "embed.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "LMS.Infrastructure", "AI", "embed.py")
        };
        _scriptPath = possiblePaths.FirstOrDefault(File.Exists) ?? possiblePaths[0];
        _modelName = configuration["Embedding:ModelName"] ?? "all-MiniLM-L6-v2";
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<float>();
        }

        if (!File.Exists(_scriptPath))
        {
            throw new FileNotFoundException("Embedding script not found", _scriptPath);
        }

        var pythonPath = await _pythonEnv.GetPythonPathAsync();

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{_scriptPath}\" --text \"{text.Replace("\"", "\\\"")}\" --model {_modelName}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(_scriptPath) ?? string.Empty,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            var error = errorBuilder.ToString();
            _logger.LogError("[PythonEmbedding] Script failed. Error: {Error}", error);
            throw new Exception($"Embedding script failed with exit code {process.ExitCode}. Error: {error}");
        }

        var output = outputBuilder.ToString();
        if (string.IsNullOrWhiteSpace(output))
        {
            return Array.Empty<float>();
        }

        try
        {
            var result = JsonSerializer.Deserialize<float[]>(output);
            return result ?? Array.Empty<float>();
        }
        catch (JsonException ex)
        {
            _logger.LogError("[PythonEmbedding] Failed to parse JSON: {Output}. Error: {Error}", output, ex.Message);
            throw new Exception($"Failed to parse Embedding JSON output. Error: {ex.Message}");
        }
    }

    public async Task UpsertBatchAsync(string collectionName, List<VectorPoint> points)
    {
        if (points == null || points.Count == 0)
            return;

        if (!File.Exists(_scriptPath))
            throw new FileNotFoundException("Embedding script not found", _scriptPath);

        var pythonPath = await _pythonEnv.GetPythonPathAsync();
        
        // Tạo file JSON tạm cho input
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        var jsonContent = JsonSerializer.Serialize(points.Select(p => new {
            id = p.Id.ToString(),
            text = p.Text,
            payload = p.Payload
        }));
        await File.WriteAllTextAsync(tempFile, jsonContent);

        // Qdrant gRPC dùng 6334, nhưng Python qdrant-client mặc định dùng HTTP trên 6333
        // Chuyển đổi URL để Python có thể kết nối qua HTTP
        var pythonQdrantUrl = _qdrantUrl.Replace(":6334", ":6333");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = pythonPath,
                Arguments = $"\"{_scriptPath}\" --input_json \"{tempFile}\" --model {_modelName} --qdrant_url \"{pythonQdrantUrl}\" --collection \"{collectionName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(_scriptPath) ?? string.Empty,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };
            var errorBuilder = new StringBuilder();

            process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = errorBuilder.ToString();
                _logger.LogError("[PythonEmbedding] Batch Upsert failed. Error: {Error}", error);
                throw new Exception($"Batch Upsert failed with exit code {process.ExitCode}. Error: {error}");
            }
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
