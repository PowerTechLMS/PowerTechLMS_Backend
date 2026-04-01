namespace LMS.Core.Interfaces;

public interface IPythonEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text);

    Task UpsertBatchAsync(string collectionName, List<VectorPoint> points);
}

public class VectorPoint
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = string.Empty;

    public Dictionary<string, object> Payload { get; set; } = new();
}
