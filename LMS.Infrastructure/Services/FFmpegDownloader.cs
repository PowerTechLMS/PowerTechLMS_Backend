using System.IO.Compression;

namespace LMS.Infrastructure.Services;

public interface IFFmpegDownloader
{
    Task<string> GetFFmpegPathAsync();
    Task<string> GetFFprobePathAsync();
}

public class FFmpegDownloader : IFFmpegDownloader
{
    private const string DownloadUrl = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";
    private readonly string _basePath;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public FFmpegDownloader()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "External", "ffmpeg");
        _ffmpegPath = Path.Combine(_basePath, "bin", "ffmpeg.exe");
        _ffprobePath = Path.Combine(_basePath, "bin", "ffprobe.exe");
    }

    public async Task<string> GetFFmpegPathAsync()
    {
        if (File.Exists(_ffmpegPath)) return _ffmpegPath;
        await EnsureDownloadedAsync();
        return _ffmpegPath;
    }

    public async Task<string> GetFFprobePathAsync()
    {
        if (File.Exists(_ffprobePath)) return _ffprobePath;
        await EnsureDownloadedAsync();
        return _ffprobePath;
    }

    private async Task EnsureDownloadedAsync()
    {
        if (File.Exists(_ffmpegPath) && File.Exists(_ffprobePath)) return;

        if (!Directory.Exists(_basePath)) Directory.CreateDirectory(_basePath);

        var zipPath = Path.Combine(_basePath, "ffmpeg.zip");
        Console.WriteLine($"Downloading FFmpeg from {DownloadUrl}...");
        
        using (var httpClient = new HttpClient())
        {
            var responseStream = await httpClient.GetStreamAsync(DownloadUrl);
            using (var fs = new FileStream(zipPath, FileMode.Create))
            {
                await responseStream.CopyToAsync(fs);
            }
        }

        Console.WriteLine("Extracting FFmpeg...");
        var extractPath = Path.Combine(_basePath, "temp_extract");
        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
        
        ZipFile.ExtractToDirectory(zipPath, extractPath);

        // Find the bin folder inside the extracted content
        var binFolder = Directory.GetDirectories(extractPath)
            .SelectMany(d => Directory.GetDirectories(d, "bin"))
            .FirstOrDefault();

        if (binFolder != null)
        {
            var targetBin = Path.Combine(_basePath, "bin");
            if (!Directory.Exists(targetBin)) Directory.CreateDirectory(targetBin);

            File.Copy(Path.Combine(binFolder, "ffmpeg.exe"), _ffmpegPath, true);
            File.Copy(Path.Combine(binFolder, "ffprobe.exe"), _ffprobePath, true);
        }

        // Cleanup
        File.Delete(zipPath);
        Directory.Delete(extractPath, true);
        Console.WriteLine("FFmpeg setup complete.");
    }
}
