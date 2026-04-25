using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
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
    private readonly IHostEnvironment _hostEnv;
    private readonly IServer _server;
    private readonly IHostApplicationLifetime _lifetime;
    private Process? _sidecarProcess;

    public string SidecarUrl { get; private set; } = "http://localhost:8000";

    public AiSidecarManager(
        IPythonEnvService pythonEnv,
        IConfiguration config,
        ILogger<AiSidecarManager> logger,
        IHostEnvironment hostEnv,
        IServer server,
        IHostApplicationLifetime lifetime)
    {
        _pythonEnv = pythonEnv;
        _config = config;
        _logger = logger;
        _hostEnv = hostEnv;
        _server = server;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _lifetime.ApplicationStarted.Register(() => Task.Run(async () => await StartSidecarProcessAsync()));
        return Task.CompletedTask;
    }

    private async Task StartSidecarProcessAsync()
    {
        _logger.LogInformation("[AiSidecar] Đang chuẩn bị khởi động AI Sidecar...");

        var port = GetFreePort();
        SidecarUrl = $"http://localhost:{port}";

        var pythonExe = await _pythonEnv.GetPythonPathAsync();

        var searchPaths = new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        string? sidecarDir = null;

        foreach(var startPath in searchPaths)
        {
            var checkDir = startPath;
            while(!string.IsNullOrEmpty(checkDir))
            {
                var potential = Path.Combine(checkDir, "ai_sidecar");
                if(Directory.Exists(potential))
                {
                    sidecarDir = potential;
                    break;
                }
                var parent = Directory.GetParent(checkDir);
                if(parent == null || parent.FullName == checkDir)
                    break;
                checkDir = parent.FullName;
            }
            if(sidecarDir != null)
                break;
        }

        if(sidecarDir == null)
        {
            _logger.LogError(
                "[AiSidecar] Không tìm thấy thư mục ai_sidecar tại {BaseDir} hoặc các thư mục cha.",
                AppContext.BaseDirectory);
            return;
        }

        var mainPy = Path.Combine(sidecarDir, "main.py");
        if(!File.Exists(mainPy))
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

        startInfo.EnvironmentVariables["OPENAI_API_KEY"] = _config["LlmSettings:ApiKey"];
        startInfo.EnvironmentVariables["OPENAI_API_BASE"] = _config["LlmSettings:ApiUrl"] + "/v1";

        var backendUrl = GetInternalBackendUrl();
        var llmUrl = _config["LlmSettings:ApiUrl"];

        _logger.LogWarning("[AiSidecar] ==================================================");
        _logger.LogWarning("[AiSidecar] CẤU HÌNH KẾT NỐI:");
        _logger.LogWarning("[AiSidecar] - Backend nội bộ: {Url}", backendUrl);
        _logger.LogWarning("[AiSidecar] - Model LLM: {Url}", llmUrl);
        _logger.LogWarning("[AiSidecar] ==================================================");

        startInfo.EnvironmentVariables["BACKEND_URL"] = $"{backendUrl}/api";
        startInfo.EnvironmentVariables["BACKEND_INTERNAL_SECRET"] = _config["Jwt:Secret"];
        startInfo.EnvironmentVariables["PORT"] = port.ToString();
        startInfo.EnvironmentVariables["PYTHONPATH"] = sidecarDir;

        try
        {
            _sidecarProcess = new Process { StartInfo = startInfo };
            _sidecarProcess.OutputDataReceived += (s, e) =>
            {
                if(e.Data != null)
                    _logger.LogInformation("[Python-Sidecar] {Msg}", e.Data);
            };
            _sidecarProcess.ErrorDataReceived += (s, e) =>
            {
                if(e.Data != null)
                    _logger.LogWarning("[Python-Sidecar-Err] {Msg}", e.Data);
            };

            _sidecarProcess.Start();
            _sidecarProcess.BeginOutputReadLine();
            _sidecarProcess.BeginErrorReadLine();

            _logger.LogInformation(
                "[AiSidecar] AI Sidecar đã khởi chạy thành công tại {Url} (PID: {Pid})",
                SidecarUrl,
                _sidecarProcess.Id);
        } catch(Exception ex)
        {
            _logger.LogError(ex, "[AiSidecar] Lỗi khi khởi chạy AI Sidecar.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if(_sidecarProcess != null && !_sidecarProcess.HasExited)
        {
            _logger.LogInformation("[AiSidecar] Đang đóng AI Sidecar...");
            try
            {
                _sidecarProcess.Kill(true);
            } catch(Exception ex)
            {
                _logger.LogWarning(ex, "[AiSidecar] Lỗi khi đóng tiến trình Sidecar.");
            }
        }
        return Task.CompletedTask;
    }

    private string GetInternalBackendUrl()
    {
        var addressFeature = _server.Features.Get<IServerAddressesFeature>();
        if(addressFeature != null && addressFeature.Addresses.Count > 0)
        {
            var address = addressFeature.Addresses.FirstOrDefault(a => a.StartsWith("http://")) ??
                addressFeature.Addresses.First();

            _logger.LogInformation("[AiSidecar] Server đang lắng nghe tại: {Addr}", address);

            return address
                .Replace("*", "localhost")
                .Replace("0.0.0.0", "localhost")
                .Replace("[::]", "localhost");
        }

        return "http://localhost:9999";
    }

    private int GetFreePort()
    {
        var configPort = _config["AiSidecar:Port"];
        if(int.TryParse(configPort, out int p))
            return p;

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }
}
