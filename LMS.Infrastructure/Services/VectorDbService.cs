using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartComponents.LocalEmbeddings;

namespace LMS.Infrastructure.Services;

public class VectorDbService
{
    private readonly QdrantClient _client;
    private readonly LocalEmbedder _embedder;
    private const string CollectionName = "powertech_knowledge";

    public VectorDbService(string qdrantHost, int qdrantPort)
    {
        _client = new QdrantClient(qdrantHost, qdrantPort);
        _embedder = new LocalEmbedder();
    }

    private bool _collectionVerified = false;

    public async Task UpsertVectorAsync(Guid pointId, string content, object metadata)
    {
        var embedding = _embedder.Embed(content);

        if(!_collectionVerified)
        {
            try
            {
                await EnsureCollectionExistsAsync();
                _collectionVerified = true;
            } catch
            {
                throw;
            }
        }
        var point = new PointStruct
        {
            Id = pointId,
            Vectors = embedding.Values.ToArray(),
            Payload = { ["content"] = content, ["metadata"] = metadata?.ToString() ?? "{}" }
        };

        await _client.UpsertAsync(CollectionName, new[] { point });
    }

    private async Task EnsureCollectionExistsAsync()
    {
        var collections = await _client.ListCollectionsAsync();
        if(!collections.Contains(CollectionName))
        {
            await _client.CreateCollectionAsync(
                CollectionName,
                new VectorParams { Size = 384, Distance = Distance.Cosine });
        }
    }
}
