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
    private readonly Lazy<LocalEmbedder?> _embedder;
    private const string CollectionName = "powertech_knowledge";
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    public VectorDbService(string qdrantUrl)
    {
        _client = new QdrantClient(new Uri(qdrantUrl));
        _embedder = new Lazy<LocalEmbedder?>(
            () =>
            {
                try
                {
                    return new LocalEmbedder();
                } catch
                {
                    return null;
                }
            });
        
        // Khởi động chủ động ngay khi service được tạo
        _ = EnsureCollectionReadyAsync();
    }

    public bool IsEmbeddingAvailable => _embedder.Value is not null;

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
            if(!IsEmbeddingAvailable)
                return;

            var embedding = _embedder.Value!.Embed(content);
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
            if(ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                _collectionVerified = false;
            }
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
            if(!IsEmbeddingAvailable)
                return new List<(Guid Id, string Content, string Metadata)>();

            var embedding = _embedder.Value!.Embed(query);

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
            if(!IsEmbeddingAvailable)
                return new List<(Guid Id, string Content)>();

            var embedding = _embedder.Value!.Embed(query);

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

        await _initializationLock.WaitAsync();
        try
        {
            if(_collectionVerified)
                return true;

            if(_serviceUnreachable && DateTime.UtcNow - _lastRetryTime < RetryInterval)
            {
                return false;
            }

            try
            {
                await EnsureCollectionExistsAsync();
                _collectionVerified = true;
                _serviceUnreachable = false;
                return true;
            } catch(Exception ex)
            {
                // Nếu lỗi là do collection đã tồn tại thì coi như đã sẵn sàng
                if(ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    _collectionVerified = true;
                    _serviceUnreachable = false;
                    return true;
                }

                if(!_serviceUnreachable)
                {
                    Console.WriteLine($"[VectorDbService] Note: Qdrant (VectorDB) is not available or connection failed: {ex.Message}. AI-driven search features will be disabled. (Message suppressed for future retries)");
                }

                _serviceUnreachable = true;
                _lastRetryTime = DateTime.UtcNow;
                return false;
            }
        } finally
        {
            _initializationLock.Release();
        }
    }

    private async Task EnsureCollectionExistsAsync()
    {
        var collections = await _client.ListCollectionsAsync();
        if(!collections.Any(c => string.Equals(c, CollectionName, StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                await _client.CreateCollectionAsync(
                    CollectionName,
                    new VectorParams { Size = 384, Distance = Distance.Cosine });
            } catch(Exception ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                // Bỏ qua nếu collection đã được tạo bởi tiến trình khác
            }
        }
    }
}
