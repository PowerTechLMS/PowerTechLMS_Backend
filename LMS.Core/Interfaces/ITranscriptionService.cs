
namespace LMS.Core.Interfaces;

public interface ITranscriptionService
{
    Task<List<TextSegment>> TranscribeAsync(string audioPath);
}

public class TextSegment
{
    public double StartTime { get; set; }

    public double EndTime { get; set; }

    public string Text { get; set; } = string.Empty;
}
