using LMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class FasterWhisperService : ITranscriptionService
{
    private readonly IPythonEnvService _pythonEnv;
    private readonly ILogger<FasterWhisperService> _logger;
    private readonly string _scriptPath;
    private readonly string _modelName;

    public FasterWhisperService(
        IPythonEnvService pythonEnv,
        IConfiguration configuration,
        ILogger<FasterWhisperService> logger)
    {
        _pythonEnv = pythonEnv;
        _logger = logger;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(baseDir, "AI", "transcribe.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "AI", "transcribe.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "LMS.Infrastructure", "AI", "transcribe.py"),
            Path.Combine(Directory.GetCurrentDirectory(), "LMS.Infrastructure", "AI", "transcribe.py")
        };
        _scriptPath = possiblePaths.FirstOrDefault(File.Exists) ?? possiblePaths[0];
        _modelName = configuration["FasterWhisper:ModelName"] ?? "small";
    }

    public async Task<List<TextSegment>> TranscribeAsync(string audioPath)
    {
        if(!File.Exists(audioPath))
        {
            throw new FileNotFoundException("Audio file not found for transcription", audioPath);
        }

        if(!File.Exists(_scriptPath))
        {
            throw new FileNotFoundException("Transcription script not found", _scriptPath);
        }

        var pythonPath = await _pythonEnv.GetPythonPathAsync();

        _logger.LogInformation(
            "Bắt đầu gọi Faster-Whisper. Script: {ScriptPath}, Audio: {AudioPath}, Model: {Model}",
            _scriptPath,
            audioPath,
            _modelName);

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = $"\"{_scriptPath}\" --input \"{audioPath}\" --model {_modelName}",
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

        process.OutputDataReceived += (s, e) =>
        {
            if(e.Data != null)
                outputBuilder.AppendLine(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if(e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
                _logger.LogInformation("[Python Log] {Log}", e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var output = outputBuilder.ToString();
        var error = errorBuilder.ToString();

        if(process.ExitCode != 0)
        {
            throw new Exception($"Faster-Whisper failed with exit code {process.ExitCode}. Error: {error}");
        }

        if(string.IsNullOrWhiteSpace(output))
        {
            return new List<TextSegment>();
        }

        try
        {
            var results = JsonSerializer.Deserialize<List<WhisperResult>>(
                output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return results?.Select(r => new TextSegment { StartTime = r.Start, EndTime = r.End, Text = r.Text })
                    .ToList() ??
                new List<TextSegment>();
        } catch(JsonException ex)
        {
            throw new Exception($"Failed to parse Whisper JSON output. Output: {output}. Error: {ex.Message}");
        }
    }

    private class WhisperResult
    {
        public double Start { get; set; }

        public double End { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}
