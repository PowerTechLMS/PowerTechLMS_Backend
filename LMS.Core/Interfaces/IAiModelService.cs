
namespace LMS.Core.Interfaces;

public interface IAiModelService
{
    Task<string> RefineTextAsync(string rawText);

    Task<List<string>> RefineTextBatchAsync(List<string> texts);
}
