using LMS.Core.Interfaces;
using Whisper.net;
using Whisper.net.Ggml;

namespace LMS.Infrastructure.Services;

public class WhisperService : ITranscriptionService
{
    private readonly string _modelPath;

    public WhisperService(string modelPath) { _modelPath = modelPath; }

    public async Task<List<TextSegment>> TranscribeAsync(string audioPath)
    {
        var segments = new List<TextSegment>();

        if(!File.Exists(_modelPath))
        {
            var dir = Path.GetDirectoryName(_modelPath);
            if(!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
            using var fileWriter = File.OpenWrite(_modelPath);
            await modelStream.CopyToAsync(fileWriter);
        }

        using var factory = WhisperFactory.FromPath(_modelPath);
        using var processor = factory.CreateBuilder()
            .WithLanguage("vi")
            .WithThreads(Math.Max(2, Environment.ProcessorCount / 2)) // Tối ưu số luồng theo CPU
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

        return segments;
    }
}
