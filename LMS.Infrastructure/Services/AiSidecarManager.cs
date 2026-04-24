using LMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LMS.Infrastructure.Services;

public interface IAiSidecarManager
{
    string SidecarUrl { get; }
}

public class AiSidecarManager : IHostedService, IAiSidecarManager
{
    private readonly IPythonEnvService _pythonEnv;
    private readonly IConfiguration _config;
    private readonly ILogger<AiSidecarManager> _logger;
    private Process? _sidecarProcess;
    private string _sidecarUrl = "http://localhost:8000";

    public string SidecarUrl => _sidecarUrl;

    public AiSidecarManager(
        IPythonEnvService pythonEnv,
        IConfiguration config,
        ILogger<AiSidecarManager> logger)
    {
        _pythonEnv = pythonEnv;
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[AiSidecar] Đang chuẩn bị khởi động AI Sidecar...");

        var port = GetFreePort();
        _sidecarUrl = $"http://localhost:{port}";
        
        var pythonExe = await _pythonEnv.GetPythonPathAsync();
        
        // Tự động tìm thư mục ai_sidecar: Ưu tiên ở BaseDirectory hoặc CurrentDirectory và đi ngược lên
        var searchPaths = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        string? sidecarDir = null;

        foreach (var startPath in searchPaths)
        {
            var checkDir = startPath;
            while (!string.IsNullOrEmpty(checkDir))
            {
                var potential = Path.Combine(checkDir, "ai_sidecar");
                if (Directory.Exists(potential))
                {
                    sidecarDir = potential;
                    break;
                }
                var parent = Directory.GetParent(checkDir);
                if (parent == null || parent.FullName == checkDir) break;
                checkDir = parent.FullName;
            }
            if (sidecarDir != null) break;
        }
        
        if (sidecarDir == null)
        {
            _logger.LogError("[AiSidecar] Không tìm thấy thư mục ai_sidecar tại {BaseDir} hoặc các thư mục cha.", AppContext.BaseDirectory);
            return;
        }

        _logger.LogInformation("[AiSidecar] AI Sidecar đã được tìm thấy tự động tại: {Path}", Path.GetFullPath(sidecarDir));

        var mainPy = Path.Combine(sidecarDir, "main.py");

        if (!File.Exists(mainPy))
        {
            _logger.LogError("[AiSidecar] Không tìm thấy file main.py tại {Path}", mainPy);
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{mainPy}\"",
            WorkingDirectory = sidecarDir,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Truyền cấu hình qua biến môi trường
        startInfo.EnvironmentVariables["OPENAI_API_KEY"] = _config["LlmSettings:ApiKey"];
        startInfo.EnvironmentVariables["OPENAI_API_BASE"] = _config["LlmSettings:ApiUrl"] + "/v1";
        
        // Xác định Backend URL động (Ưu tiên từ config, nếu không có thì mặc định cổng 5100)
        var backendUrl = _config["FrontendUrl"]?.Replace("https://powertech.io.vn", "http://localhost:5100") ?? "http://localhost:5100";
        startInfo.EnvironmentVariables["BACKEND_URL"] = $"{backendUrl}/api";
        startInfo.EnvironmentVariables["BACKEND_INTERNAL_SECRET"] = _config["Jwt:Secret"];
        startInfo.EnvironmentVariables["PORT"] = port.ToString();
        startInfo.EnvironmentVariables["PYTHONPATH"] = sidecarDir;

        try
        {
            _sidecarProcess = new Process { StartInfo = startInfo };
            _sidecarProcess.OutputDataReceived += (s, e) => { if (e.Data != null) _logger.LogInformation("[Python-Sidecar] {Msg}", e.Data); };
            _sidecarProcess.ErrorDataReceived += (s, e) => { if (e.Data != null) _logger.LogWarning("[Python-Sidecar-Err] {Msg}", e.Data); };

            _sidecarProcess.Start();
            _sidecarProcess.BeginOutputReadLine();
            _sidecarProcess.BeginErrorReadLine();

            _logger.LogInformation("[AiSidecar] AI Sidecar đã khởi chạy thành công tại {Url} (PID: {Pid})", _sidecarUrl, _sidecarProcess.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AiSidecar] Lỗi khi khởi chạy AI Sidecar.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_sidecarProcess != null && !_sidecarProcess.HasExited)
        {
            _logger.LogInformation("[AiSidecar] Đang đóng AI Sidecar...");
            try
            {
                _sidecarProcess.Kill(true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AiSidecar] Lỗi khi đóng tiến trình Sidecar.");
            }
        }
        return Task.CompletedTask;
    }

    private int GetFreePort()
    {
        // Kiểm tra xem có cấu hình port cố định không, nếu không mới lấy port động
        var configPort = _config["AiSidecar:Port"];
        if (int.TryParse(configPort, out int p)) return p;

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
