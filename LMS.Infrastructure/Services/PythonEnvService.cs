using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LMS.Infrastructure.Services;

public interface IPythonEnvService
{
    Task<string> GetPythonPathAsync();

    Task EnsureEnvReadyAsync();
}

public class PythonEnvService : IPythonEnvService
{
    private readonly string _basePath;
    private readonly string _venvPath;
    private readonly string _pythonExecutable;
    private readonly bool _isWindows;
    private readonly ILogger<PythonEnvService> _logger;

    public PythonEnvService(ILogger<PythonEnvService> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "External", "python_env");
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        _venvPath = _basePath;
        _pythonExecutable = _isWindows 
            ? Path.Combine(_venvPath, "Scripts", "python.exe") 
            : Path.Combine(_venvPath, "bin", "python");
    }

    public async Task<string> GetPythonPathAsync()
    {
        await EnsureEnvReadyAsync();
        return _pythonExecutable;
    }

    public async Task EnsureEnvReadyAsync()
    {
        bool venvExists = File.Exists(_pythonExecutable);
        if(!venvExists)
        {
            _logger.LogInformation("[PythonEnv] Đang khởi tạo môi trường Python mới...");
            if(!Directory.Exists(_basePath))
                Directory.CreateDirectory(_basePath);

            await CreateVenvAsync();
        }

        // Marker file để đảm bảo đã cài đủ các thư viện mới nhất
        // v2: faster-whisper, sentence-transformers, qdrant-client
        string markerFile = Path.Combine(_venvPath, "requirements_v2.marker");
        if (!File.Exists(markerFile))
        {
            _logger.LogInformation("[PythonEnv] Đang cài đặt/cập nhật các thư viện phụ thuộc (Whisper, SentenceTransformers, Qdrant)...");
            await InstallDependenciesAsync();
            await File.WriteAllTextAsync(markerFile, DateTime.Now.ToString());
            _logger.LogInformation("[PythonEnv] Hoàn tất cài đặt thư viện.");
        }
    }

    private async Task CreateVenvAsync()
    {
        var pythonSystem = _isWindows ? "python" : "python3";
        var startInfo = new ProcessStartInfo
        {
            FileName = pythonSystem,
            Arguments = $"-m venv \"{_venvPath}\"",
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
            await process.WaitForExitAsync();
            if(process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new Exception($"Failed to create venv: {error}");
            }
        } catch(Exception ex)
        {
            throw new Exception($"Python not found or failed to create venv. Ensure Python is installed. Error: {ex.Message}");
        }
    }

    private async Task InstallDependenciesAsync()
    {
        var pipExecutable = _isWindows 
            ? Path.Combine(_venvPath, "Scripts", "pip.exe") 
            : Path.Combine(_venvPath, "bin", "pip");

        // Cài đặt faster-whisper, sentence-transformers và qdrant-client
        await RunCommandAsync(pipExecutable, "install faster-whisper sentence-transformers qdrant-client");

        // Cài đặt torch
        // Trên Windows mặc định dùng cu124 (GPU)
        // Trên Linux mặc định dùng cpu index để tiết kiệm dung lượng (phù hợp hosting Debian không GPU)
        var torchArgs = _isWindows 
            ? "install torch --index-url https://download.pytorch.org/whl/cu124" 
            : "install torch --index-url https://download.pytorch.org/whl/cpu";
        await RunCommandAsync(pipExecutable, torchArgs);
    }

    private async Task RunCommandAsync(string fileName, string arguments)
    {
        string tempDir = Path.Combine(_basePath, "temp");
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.EnvironmentVariables["TMPDIR"] = tempDir;
        startInfo.EnvironmentVariables["TEMP"] = tempDir;
        startInfo.EnvironmentVariables["TMP"] = tempDir;
        startInfo.EnvironmentVariables["PIP_CACHE_DIR"] = Path.Combine(_basePath, "pip_cache");

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        await process.WaitForExitAsync();

        if(process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Command failed: {fileName} {arguments}. Error: {error}");
        }
    }
}
