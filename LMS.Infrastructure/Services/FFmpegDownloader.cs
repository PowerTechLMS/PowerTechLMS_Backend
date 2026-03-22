using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace LMS.Infrastructure.Services;

public interface IFFmpegDownloader
{
    Task<string> GetFFmpegPathAsync();

    Task<string> GetFFprobePathAsync();
}

public class FFmpegDownloader : IFFmpegDownloader
{
    private readonly string _basePath;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;
    private readonly bool _isWindows;
    private readonly ILogger<FFmpegDownloader> _logger;

    public FFmpegDownloader(ILogger<FFmpegDownloader> logger)
    {
        _logger = logger;
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "External", "ffmpeg");
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var ext = _isWindows ? ".exe" : string.Empty;
        _ffmpegPath = Path.Combine(_basePath, "bin", $"ffmpeg{ext}");
        _ffprobePath = Path.Combine(_basePath, "bin", $"ffprobe{ext}");
    }

    public async Task<string> GetFFmpegPathAsync()
    {
        if(File.Exists(_ffmpegPath))
            return _ffmpegPath;
        await EnsureDownloadedAsync();
        return _ffmpegPath;
    }

    public async Task<string> GetFFprobePathAsync()
    {
        if(File.Exists(_ffprobePath))
            return _ffprobePath;
        await EnsureDownloadedAsync();
        return _ffprobePath;
    }

    private async Task EnsureDownloadedAsync()
    {
        if(File.Exists(_ffmpegPath) && File.Exists(_ffprobePath))
            return;

        if(!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            await DownloadAndExtractWindowsAsync();
        } else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            await DownloadAndExtractMacAsync();
        } else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            await DownloadAndExtractLinuxAsync();
        } else
        {
            throw new NotSupportedException("Unsupported Operating System.");
        }

        SetExecutablePermission(_ffmpegPath);
        SetExecutablePermission(_ffprobePath);
    }

    private async Task DownloadAndExtractWindowsAsync()
    {
        const string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
        var zipPath = Path.Combine(_basePath, "ffmpeg.zip");
        _logger.LogInformation($"Downloading FFmpeg for Windows from {DownloadUrl}...");

        using(var httpClient = new HttpClient())
        {
            var responseStream = await httpClient.GetStreamAsync(DownloadUrl);
            using(var fs = new FileStream(zipPath, FileMode.Create))
            {
                await responseStream.CopyToAsync(fs);
            }
        }

        _logger.LogInformation("Extracting FFmpeg for Windows...");
        var extractPath = Path.Combine(_basePath, "temp_extract");
        if(Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);

        ZipFile.ExtractToDirectory(zipPath, extractPath);

        var binFolder = Directory.GetDirectories(extractPath)
            .SelectMany(d => Directory.GetDirectories(d, "bin"))
            .FirstOrDefault();

        if(binFolder != null)
        {
            var targetBin = Path.Combine(_basePath, "bin");
            if(!Directory.Exists(targetBin))
                Directory.CreateDirectory(targetBin);

            File.Copy(Path.Combine(binFolder, "ffmpeg.exe"), _ffmpegPath, true);
            File.Copy(Path.Combine(binFolder, "ffprobe.exe"), _ffprobePath, true);
        }

        File.Delete(zipPath);
        if(Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);
        _logger.LogInformation("FFmpeg setup complete for Windows.");
    }

    private async Task DownloadAndExtractMacAsync()
    {
        const string FfmpegUrl = "https://evermeet.cx/ffmpeg/getrelease/zip";
        const string FfprobeUrl = "https://evermeet.cx/ffmpeg/getrelease/ffprobe/zip";

        var targetBin = Path.Combine(_basePath, "bin");
        if(!Directory.Exists(targetBin))
            Directory.CreateDirectory(targetBin);

        await DownloadAndExtractMacBinaryAsync(FfmpegUrl, "ffmpeg.zip", _ffmpegPath);
        await DownloadAndExtractMacBinaryAsync(FfprobeUrl, "ffprobe.zip", _ffprobePath);

        _logger.LogInformation("FFmpeg setup complete for MacOS.");
    }

    private async Task DownloadAndExtractMacBinaryAsync(string url, string zipName, string targetPath)
    {
        var zipPath = Path.Combine(_basePath, zipName);
        _logger.LogInformation($"Downloading {zipName} from {url}...");

        using(var httpClient = new HttpClient())
        {
            var responseStream = await httpClient.GetStreamAsync(url);
            using(var fs = new FileStream(zipPath, FileMode.Create))
            {
                await responseStream.CopyToAsync(fs);
            }
        }

        _logger.LogInformation($"Extracting {zipName}...");
        var extractPath = Path.Combine(_basePath, $"temp_extract_{zipName}");
        if(Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);

        ZipFile.ExtractToDirectory(zipPath, extractPath);

        var binaryFile = Directory.GetFiles(extractPath).FirstOrDefault();
        if(binaryFile != null)
        {
            File.Copy(binaryFile, targetPath, true);
        }

        File.Delete(zipPath);
        if(Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);
    }

    private async Task DownloadAndExtractLinuxAsync()
    {
        const string DownloadUrl = "https://johnvansickle.com/ffmpeg/releases/ffmpeg-release-amd64-static.tar.xz";
        var tarPath = Path.Combine(_basePath, "ffmpeg.tar.xz");
        _logger.LogInformation($"Downloading FFmpeg for Linux from {DownloadUrl}...");

        using(var httpClient = new HttpClient())
        {
            var responseStream = await httpClient.GetStreamAsync(DownloadUrl);
            using(var fs = new FileStream(tarPath, FileMode.Create))
            {
                await responseStream.CopyToAsync(fs);
            }
        }

        _logger.LogInformation("Extracting FFmpeg for Linux with tar...");
        var extractPath = Path.Combine(_basePath, "temp_extract_linux");
        if(Directory.Exists(extractPath))
            Directory.Delete(extractPath, true);
        Directory.CreateDirectory(extractPath);

        var process = new Process
        {
            StartInfo =
                new ProcessStartInfo
                {
                    FileName = "tar",
                    Arguments = $"-xJf \"{tarPath}\" -C \"{extractPath}\"",
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
        };

        var errorLog = new StringBuilder();
        process.ErrorDataReceived += (s, e) =>
        {
            if(e.Data != null)
                errorLog.AppendLine(e.Data);
        };

        try
        {
            process.Start();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if(process.ExitCode != 0)
            {
                throw new Exception($"Tar extraction failed with exit code {process.ExitCode}. Error: {errorLog}");
            }

            var extractedDir = Directory.GetDirectories(extractPath).FirstOrDefault();
            if(extractedDir != null)
            {
                var targetBin = Path.Combine(_basePath, "bin");
                if(!Directory.Exists(targetBin))
                    Directory.CreateDirectory(targetBin);

                var sourceFfmpeg = Path.Combine(extractedDir, "ffmpeg");
                var sourceFfprobe = Path.Combine(extractedDir, "ffprobe");

                if(File.Exists(sourceFfmpeg))
                    File.Copy(sourceFfmpeg, _ffmpegPath, true);
                if(File.Exists(sourceFfprobe))
                    File.Copy(sourceFfprobe, _ffprobePath, true);
            }
        } finally
        {
            File.Delete(tarPath);
            if(Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);
        }

        _logger.LogInformation("FFmpeg setup complete for Linux.");
    }

    private void SetExecutablePermission(string path)
    {
        if(!_isWindows && File.Exists(path))
        {
            try
            {
                var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "chmod",
                        Arguments = $"+x \"{path}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                process?.WaitForExit();
                _logger.LogInformation($"Set executable permission for {path}");
            } catch(Exception ex)
            {
                _logger.LogWarning($"Failed to set executable permission for {path}: {ex.Message}");
            }
        }
    }
}
