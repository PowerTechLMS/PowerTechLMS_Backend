using System;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartComponents.LocalEmbeddings;

namespace LMS.Infrastructure.Services;

public class VectorDbService
{
    private readonly QdrantClient _client;
    private readonly LocalEmbedder _embedder;
    private const string CollectionName = "powertech_knowledge";

    public VectorDbService(string qdrantUrl)
    {
        _client = new QdrantClient(new Uri(qdrantUrl));
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

    public async Task<List<(Guid Id, string Content, string Metadata)>> SearchAsync(string query, int lessonId, int limit = 5)
    {
        var embedding = _embedder.Embed(query);

        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "metadata",
                Match = new Match { Text = $"LessonId = {lessonId}," }
            }
        });

        var results = await _client.SearchAsync(
            CollectionName,
            embedding.Values.ToArray(),
            filter: filter,
            limit: (ulong)limit
        );

        return results.Select(r => (
            Guid.Parse(r.Id.Uuid),
            r.Payload["content"].StringValue,
            r.Payload["metadata"].StringValue
        )).ToList();
    }

    public async Task DeleteVectorsByFilterAsync(string key, object value)
    {
        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "metadata",
                Match = new Match { Text = $"{key} = {value}," }
            }
        });

        await _client.DeleteAsync(CollectionName, filter: filter);
    }


    public async Task<List<(string Content, string Metadata)>> GetAllSegmentsAsync(int lessonId)
    {
        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "metadata",
                Match = new Match { Text = $"LessonId = {lessonId}," }
            }
        });

        // Scroll trả về ScrollResponse, danh sách point nằm trong Result
        var results = await _client.ScrollAsync(CollectionName, filter: filter, limit: 100);

        return results.Result.Select(p => (
            p.Payload["content"].StringValue,
            p.Payload["metadata"].StringValue
        )).ToList();
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
