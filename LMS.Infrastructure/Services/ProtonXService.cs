using LMS.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace LMS.Infrastructure.Services;

public class ProtonXService : IAiModelService
{
    private readonly string _modelDir;
    private readonly string _modelPath;
    private readonly ILogger<ProtonXService> _logger;

    public ProtonXService(string modelDir, ILogger<ProtonXService> logger)
    {
        _modelDir = modelDir;
        _logger = logger;
        _modelPath = Path.Combine(modelDir, "config.json");

        EnsureModelReadyAsync().Wait();
    }

    private async Task EnsureModelReadyAsync()
    {
        var scriptPath = GetScriptPath();
        if(!File.Exists(scriptPath))
        {
            throw new Exception($"CRITICAL: Script chuẩn hoá {scriptPath} không tìm thấy.");
        }
    }

    private string GetScriptPath()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", "refine_cli.py");
        if(!File.Exists(path))
        {
            path = Path.Combine(Directory.GetCurrentDirectory(), "..", "LMS.Infrastructure", "Scripts", "refine_cli.py");
        }
        return path;
    }


    public async Task<string> RefineTextAsync(string rawText)
    {
        var results = await RefineTextBatchAsync(new List<string> { rawText });
        return results.FirstOrDefault() ?? rawText;
    }

    public async Task<List<string>> RefineTextBatchAsync(List<string> texts)
    {
        if(texts == null || !texts.Any())
            return new List<string>();

        var scriptPath = GetScriptPath();
        var tempInput = Path.Combine(Path.GetTempPath(), $"protonx_in_{Guid.NewGuid():N}.txt");

        try
        {
            await File.WriteAllLinesAsync(
                tempInput,
                texts.Select(t => (t ?? string.Empty).Replace("\r", " ").Replace("\n", " ").Trim()));

            var startInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{scriptPath}\" --batch \"{tempInput}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            var outputTask = process!.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            string output = await outputTask;
            string error = await errorTask;

            if(process.ExitCode != 0)
            {
                return texts;
            }

            var results = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(line => !string.IsNullOrEmpty(line))
                .ToList();

            if(results.Count >= texts.Count)
            {
                return results.Take(texts.Count).ToList();
            }

            return texts;
        } catch
        {
            return texts;
        } finally
        {
            if(File.Exists(tempInput))
                File.Delete(tempInput);
        }
    }
}
