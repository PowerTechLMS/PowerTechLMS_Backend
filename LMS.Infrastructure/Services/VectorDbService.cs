using Qdrant.Client;
using Qdrant.Client.Grpc;
using SmartComponents.LocalEmbeddings;
using System;
using System.Linq;
using System.Text.Json;

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
    private bool _serviceUnreachable = false;
    private DateTime _lastRetryTime = DateTime.MinValue;
    private static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(2);

    public async Task UpsertVectorAsync(Guid pointId, string content, IDictionary<string, object> metadata)
    {
        if(!await EnsureCollectionReadyAsync())
            return;
        try
        {
            var embedding = _embedder.Embed(content);
            var point = new PointStruct
            {
                Id = pointId,
                Vectors = embedding.Values.ToArray(),
                Payload = { ["content"] = content }
            };

            if(metadata != null)
            {
                point.Payload["metadata"] = JsonSerializer.Serialize(metadata);

                foreach(var kvp in metadata)
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
        } catch(Exception ex)
        {
            Console.WriteLine($"[VectorDbService] Error in UpsertVectorAsync: {ex.Message}");
        }
    }

    public async Task<List<(Guid Id, string Content, string Metadata)>> SearchAsync(
        string query,
        int lessonId,
        int limit = 5)
    {
        if(!await EnsureCollectionReadyAsync())
            return new List<(Guid Id, string Content, string Metadata)>();

        try
        {
            var embedding = _embedder.Embed(query);

            var filter = new Filter();
            filter.Must
                .Add(
                    new Condition
                    {
                        Field = new FieldCondition { Key = "LessonId", Match = new Match { Integer = lessonId } }
                    });

            var results = await _client.SearchAsync(
                CollectionName,
                embedding.Values.ToArray(),
                filter: filter,
                limit: (ulong)limit);

            return results.Select(
                r =>
                {
                    Guid id = Guid.Parse(r.Id.Uuid);
                    string content = r.Payload.ContainsKey("content") ? r.Payload["content"].StringValue : string.Empty;
                    string metadata;
                    if(r.Payload.ContainsKey("metadata"))
                    {
                        metadata = r.Payload["metadata"].StringValue;
                    } else
                    {
                        var dict = r.Payload
                            .Where(kvp => kvp.Key != "content")
                            .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.StringValue);
                        metadata = JsonSerializer.Serialize(dict);
                    }
                    return (id, content, metadata);
                })
                .ToList();
        } catch(Exception ex)
        {
            Console.WriteLine($"[VectorDbService] Error in SearchAsync: {ex.Message}");
            return new List<(Guid Id, string Content, string Metadata)>();
        }
    }

    public async Task<List<(Guid Id, string Content)>> SearchByDocumentAsync(
        string query,
        int documentId,
        int limit = 5)
    {
        if(!await EnsureCollectionReadyAsync())
            return new List<(Guid Id, string Content)>();

        try
        {
            var embedding = _embedder.Embed(query);

            var filter = new Filter();
            filter.Must
                .Add(
                    new Condition
                    {
                        Field = new FieldCondition { Key = "DocumentId", Match = new Match { Integer = documentId } }
                    });

            var results = await _client.SearchAsync(
                CollectionName,
                embedding.Values.ToArray(),
                filter: filter,
                limit: (ulong)limit);

            return results.Select(
                r => (
                Guid.Parse(r.Id.Uuid),
                r.Payload.ContainsKey("content") ? r.Payload["content"].StringValue : string.Empty
            ))
                .ToList();
        } catch(Exception ex)
        {
            Console.WriteLine($"[VectorDbService] Error in SearchByDocumentAsync: {ex.Message}");
            return new List<(Guid Id, string Content)>();
        }
    }

    public async Task DeleteVectorsByFilterAsync(string key, object value)
    {
        if(!await EnsureCollectionReadyAsync())
            return;

        try
        {
            var filter = new Filter();

            if(value is int i)
            {
                filter.Must
                    .Add(
                        new Condition
                        {
                            Field = new FieldCondition { Key = key, Match = new Match { Integer = (long)i } }
                        });
            } else if(value is long l)
            {
                filter.Must
                    .Add(new Condition { Field = new FieldCondition { Key = key, Match = new Match { Integer = l } } });
            } else
            {
                filter.Must
                    .Add(
                        new Condition
                        {
                            Field = new FieldCondition { Key = key, Match = new Match { Text = value.ToString() } }
                        });
            }

            await _client.DeleteAsync(CollectionName, filter: filter);
        } catch(Exception ex)
        {
            Console.WriteLine($"[VectorDbService] Error in DeleteVectorsByFilterAsync: {ex.Message}");
        }
    }


    public async Task<List<(string Content, string Metadata)>> GetAllSegmentsAsync(int lessonId)
    {
        if(!await EnsureCollectionReadyAsync())
            return new List<(string Content, string Metadata)>();

        try
        {
            var filter = new Filter();
            filter.Must
                .Add(
                    new Condition
                    {
                        Field = new FieldCondition { Key = "LessonId", Match = new Match { Integer = lessonId } }
                    });

            var results = await _client.ScrollAsync(CollectionName, filter: filter, limit: 100);

            return results.Result
                .Select(
                    p =>
                    {
                        string content = p.Payload.ContainsKey("content")
                            ? p.Payload["content"].StringValue
                            : string.Empty;
                        string metadata;
                        if(p.Payload.ContainsKey("metadata"))
                        {
                            metadata = p.Payload["metadata"].StringValue;
                        } else
                        {
                            var dict = p.Payload
                                .Where(kvp => kvp.Key != "content")
                                .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value.StringValue);
                            metadata = JsonSerializer.Serialize(dict);
                        }
                        return (content, metadata);
                    })
                .ToList();
        } catch(Exception ex)
        {
            Console.WriteLine($"[VectorDbService] Error in GetAllSegmentsAsync: {ex.Message}");
            return new List<(string Content, string Metadata)>();
        }
    }

    private async Task<bool> EnsureCollectionReadyAsync()
    {
        if(_collectionVerified)
            return true;

        if(_serviceUnreachable && DateTime.Now - _lastRetryTime < RetryInterval)
        {
            return false;
        }

        if(!_serviceUnreachable)
        {
            Console.WriteLine(
                $"[VectorDbService] Note: Qdrant (VectorDB) is not available. AI-driven search features will be disabled. (Message suppressed for future retries)");
        }
        _serviceUnreachable = true;
        _lastRetryTime = DateTime.Now;
        return false;
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
