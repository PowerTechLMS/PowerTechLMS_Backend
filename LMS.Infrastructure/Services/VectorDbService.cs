using System;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartComponents.LocalEmbeddings;
using System.Text.Json;
using System.Linq;

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

    public async Task UpsertVectorAsync(Guid pointId, string content, IDictionary<string, object> metadata)
    {
        await EnsureCollectionReadyAsync();
        var embedding = _embedder.Embed(content);
        var point = new PointStruct
        {
            Id = pointId,
            Vectors = embedding.Values.ToArray(),
            Payload = { ["content"] = content }
        };

        if (metadata != null)
        {
            // Lưu metadata dưới dạng JSON string để tương thích ngược với các Controller hiện tại
            point.Payload["metadata"] = JsonSerializer.Serialize(metadata);

            foreach (var kvp in metadata)
            {
                point.Payload[kvp.Key] = kvp.Value switch
                {
                    string s => s,
                    int i => (long)i,
                    long l => l,
                    float f => (double)f,
                    double d => d,
                    bool b => b,
                    _ => kvp.Value?.ToString() ?? string.Empty
                };
            }
        }

        await _client.UpsertAsync(CollectionName, new[] { point });
    }

    public async Task<List<(Guid Id, string Content, string Metadata)>> SearchAsync(string query, int lessonId, int limit = 5)
    {
        await EnsureCollectionReadyAsync();
        var embedding = _embedder.Embed(query);

        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "LessonId",
                Match = new Match { Integer = lessonId }
            }
        });

        var results = await _client.SearchAsync(
            CollectionName,
            embedding.Values.ToArray(),
            filter: filter,
            limit: (ulong)limit
        );

        return results.Select(r => {
            Guid id = Guid.Parse(r.Id.Uuid);
            string content = r.Payload.ContainsKey("content") ? r.Payload["content"].StringValue : string.Empty;
            string metadata;
            if (r.Payload.ContainsKey("metadata"))
            {
                metadata = r.Payload["metadata"].StringValue;
            }
            else
            {
                var dict = r.Payload.Where(kvp => kvp.Key != "content")
                                   .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.StringValue);
                metadata = JsonSerializer.Serialize(dict);
            }
            return (id, content, metadata);
        }).ToList();
    }

    public async Task<List<(Guid Id, string Content)>> SearchByDocumentAsync(string query, int documentId, int limit = 5)
    {
        await EnsureCollectionReadyAsync();
        var embedding = _embedder.Embed(query);

        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "DocumentId",
                Match = new Match { Integer = documentId }
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
            r.Payload.ContainsKey("content") ? r.Payload["content"].StringValue : string.Empty
        )).ToList();
    }

    public async Task DeleteVectorsByFilterAsync(string key, object value)
    {
        await EnsureCollectionReadyAsync();
        var filter = new Filter();

        if (value is int i)
        {
            filter.Must.Add(new Condition
            {
                Field = new FieldCondition { Key = key, Match = new Match { Integer = (long)i } }
            });
        }
        else if (value is long l)
        {
            filter.Must.Add(new Condition
            {
                Field = new FieldCondition { Key = key, Match = new Match { Integer = l } }
            });
        }
        else
        {
            filter.Must.Add(new Condition
            {
                Field = new FieldCondition { Key = key, Match = new Match { Text = value.ToString() } }
            });
        }

        await _client.DeleteAsync(CollectionName, filter: filter);
    }


    public async Task<List<(string Content, string Metadata)>> GetAllSegmentsAsync(int lessonId)
    {
        await EnsureCollectionReadyAsync();
        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "LessonId",
                Match = new Match { Integer = lessonId }
            }
        });

        // Scroll trả về ScrollResponse, danh sách point nằm trong Result
        var results = await _client.ScrollAsync(CollectionName, filter: filter, limit: 100);

        return results.Result.Select(p => {
            string content = p.Payload.ContainsKey("content") ? p.Payload["content"].StringValue : string.Empty;
            string metadata;
            if (p.Payload.ContainsKey("metadata"))
            {
                metadata = p.Payload["metadata"].StringValue;
            }
            else
            {
                // Tái cấu trúc metadata từ các key khác (flattened keys)
                var dict = p.Payload.Where(kvp => kvp.Key != "content")
                                   .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.StringValue);
                metadata = JsonSerializer.Serialize(dict);
            }
            return (content, metadata);
        }).ToList();
    }

    private async Task EnsureCollectionReadyAsync()
    {
        if (!_collectionVerified)
        {
            await EnsureCollectionExistsAsync();
            _collectionVerified = true;
        }
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
