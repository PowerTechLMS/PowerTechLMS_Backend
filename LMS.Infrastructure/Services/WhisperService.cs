using LMS.Core.Interfaces;
using Whisper.net;
using Whisper.net.Ggml;

namespace LMS.Infrastructure.Services;

public class WhisperService : ITranscriptionService
{
    private readonly string _modelPath;
    private static readonly HttpClient _httpClient = new HttpClient();
    private static readonly SemaphoreSlim _modelLock = new SemaphoreSlim(1, 1);

    public WhisperService(string modelPath) { _modelPath = modelPath; }

    public async Task<List<TextSegment>> TranscribeAsync(string audioPath)
    {
        var segments = new List<TextSegment>();

        if(!File.Exists(_modelPath))
        {
            await _modelLock.WaitAsync();
            try
            {
                if(!File.Exists(_modelPath))
                {
                    var directoryPath = Path.GetDirectoryName(_modelPath);
                    if(!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var temporaryPath = _modelPath + ".tmp";
                    using var modelStream = await new WhisperGgmlDownloader(_httpClient).GetGgmlModelAsync(
                        GgmlType.Small);
                    using(var fileWriter = File.OpenWrite(temporaryPath))
                    {
                        await modelStream.CopyToAsync(fileWriter);
                    }
                    File.Move(temporaryPath, _modelPath, true);
                }
            } finally
            {
                _modelLock.Release();
            }
        }

        try
        {
            using var factory = WhisperFactory.FromPath(_modelPath);

            using var processor = factory.CreateBuilder()
                .WithLanguage("vi")
                .WithBeamSearchSamplingStrategy()
                .ParentBuilder
                .Build();

            using var fileStream = File.OpenRead(audioPath);

            await foreach(var result in processor.ProcessAsync(fileStream))
            {
                segments.Add(
                    new TextSegment
                    {
                        StartTime = result.Start.TotalSeconds,
                        EndTime = result.End.TotalSeconds,
                        Text = result.Text
                    });
            }
        } catch
        {
            // Fallback: segments rỗng nếu lỗi native
        }

        return segments;
    }
}