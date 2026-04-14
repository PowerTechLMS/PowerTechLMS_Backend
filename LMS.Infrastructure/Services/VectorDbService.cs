using LMS.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Linq;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class VectorDbService
{
    private readonly QdrantClient _client;
    private readonly IPythonEmbeddingService _embeddingService;
    private readonly Microsoft.Extensions.Logging.ILogger<VectorDbService> _logger;
    private const string CollectionName = "powertech_knowledge";
    private readonly SemaphoreSlim _initializationLock = new(1, 1);

    public VectorDbService(string qdrantUrl, Microsoft.Extensions.Logging.ILogger<VectorDbService> logger, IPythonEmbeddingService embeddingService)
    {
        _client = new QdrantClient(new Uri(qdrantUrl));
        _logger = logger;
        _embeddingService = embeddingService;
        
        _ = EnsureCollectionReadyAsync();
    }

    private bool _collectionVerified = false;
    private bool _serviceUnreachable = false;
    private DateTime _lastRetryTime = DateTime.MinValue;
    private static readonly TimeSpan RetryInterval = TimeSpan.FromMinutes(2);

    public async Task UpsertBatchAsync(List<VectorPoint> points)
    {
        if(!await EnsureCollectionReadyAsync())
            return;
            
        try
        {
            await _embeddingService.UpsertBatchAsync(CollectionName, points);
        } catch(Exception ex)
        {
            _logger.LogError("[VectorDb] Lỗi trong UpsertBatchAsync: {Message}", ex.Message);
            if(ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                _collectionVerified = false;
            }
        }
    }

    public async Task UpsertVectorAsync(Guid pointId, string content, IDictionary<string, object> metadata)
    {
        if(!await EnsureCollectionReadyAsync())
            return;
        try
        {
            var embedding = await _embeddingService.GetEmbeddingAsync(content);
            if (embedding.Length == 0)
            {
                _logger.LogWarning("[VectorDb] Bỏ qua Upsert vì không tạo được Embedding.");
                return;
            }

            var point = new PointStruct
            {
                Id = pointId,
                Vectors = embedding,
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
            _logger.LogError("[VectorDb] Lỗi trong UpsertVectorAsync: {Message}", ex.Message);
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
        return await SearchAsync(query, new List<int> { lessonId }, limit);
    }

    public async Task<List<(Guid Id, string Content, string Metadata)>> SearchAsync(
        string query,
        List<int> lessonIds,
        int limit = 5)
    {
        if(!await EnsureCollectionReadyAsync())
            return new List<(Guid Id, string Content, string Metadata)>();

        try
        {
            var embedding = await _embeddingService.GetEmbeddingAsync(query);
            if (embedding.Length == 0)
                return new List<(Guid Id, string Content, string Metadata)>();

            var filter = new Filter();
            foreach (var id in lessonIds)
            {
                filter.Should.Add(new Condition 
                { 
                    Field = new FieldCondition { Key = "LessonId", Match = new Match { Integer = (long)id } } 
                });
            }

            var results = await _client.SearchAsync(
                CollectionName,
                embedding,
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
                            .ToDictionary(kvp => kvp.Key, kvp => ConvertValueToObject(kvp.Value));
                        metadata = JsonSerializer.Serialize(dict);
                    }
                    return (id, content, metadata);
                })
                .ToList();
        } catch(Exception ex)
        {
            _logger.LogError("[VectorDbService] Error in SearchAsync (Multi): {Message}", ex.Message);
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
            var embedding = await _embeddingService.GetEmbeddingAsync(query);
            if (embedding.Length == 0)
                return new List<(Guid Id, string Content)>();

            var filter = new Filter();
            filter.Must
                .Add(
                    new Condition
                    {
                        Field = new FieldCondition { Key = "DocumentId", Match = new Match { Integer = documentId } }
                    });

            var results = await _client.SearchAsync(
                CollectionName,
                embedding,
                filter: filter,
                limit: (ulong)limit);

            return results.Select(
                r =>
                {
                    Guid id = Guid.Parse(r.Id.Uuid);
                    string content = r.Payload.ContainsKey("content") ? r.Payload["content"].StringValue : string.Empty;
                    var dict = r.Payload
                        .Where(kvp => kvp.Key != "content")
                        .ToDictionary(kvp => kvp.Key, kvp => ConvertValueToObject(kvp.Value));
                    return (id, content);
                })
                .ToList();
        } catch(Exception ex)
        {
            _logger.LogError("[VectorDbService] Error in SearchByDocumentAsync: {Message}", ex.Message);
            if(ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                _collectionVerified = false;
            }
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
            _logger.LogError("[VectorDbService] Error in DeleteVectorsByFilterAsync: {Message}", ex.Message);
            if(ex.Message.Contains("doesn't exist", StringComparison.OrdinalIgnoreCase) || ex.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase))
            {
                _collectionVerified = false;
            }
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
                        .ToDictionary(kvp => kvp.Key, kvp => ConvertValueToObject(kvp.Value));
                    metadata = JsonSerializer.Serialize(dict);
                }
                return (content, metadata);
            })
        .ToList();
    } catch(Exception ex)
    {
        _logger.LogError("[VectorDbService] Error in GetAllSegmentsAsync: {Message}", ex.Message);
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
            if(ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                _collectionVerified = true;
                _serviceUnreachable = false;
                return true;
            }

            if(!_serviceUnreachable)
            {
                _logger.LogWarning("[VectorDbService] Note: Qdrant (VectorDB) is not available or connection failed: {Message}. AI-driven search features will be disabled. (Message suppressed for future retries)", ex.Message);
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
    _logger.LogInformation("[VectorDb] Đang kiểm tra collection '{CollectionName}'...", CollectionName);
    var collections = await _client.ListCollectionsAsync();
    _logger.LogInformation("[VectorDb] Các collection hiện có: {Collections}", string.Join(", ", collections));

    if(!collections.Any(c => string.Equals(c, CollectionName, StringComparison.OrdinalIgnoreCase)))
    {
        _logger.LogInformation("[VectorDb] Không tìm thấy collection '{CollectionName}'. Đang tạo mới...", CollectionName);
        try
        {
            await _client.CreateCollectionAsync(
                CollectionName,
                new VectorParams { Size = 384, Distance = Distance.Cosine });
            _logger.LogInformation("[VectorDb] Đã tạo thành công collection '{CollectionName}'.", CollectionName);
        } catch(Exception ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("[VectorDb] Collection '{CollectionName}' đã được khởi tạo bởi tiến trình khác.", CollectionName);
        }
    }
    else
    {
        _logger.LogInformation("[VectorDb] Collection '{CollectionName}' đã sẵn sàng.", CollectionName);
    }
}

private object ConvertValueToObject(Value value)
{
    return value.KindCase switch
    {
        Value.KindOneofCase.StringValue => value.StringValue,
        Value.KindOneofCase.DoubleValue => value.DoubleValue,
        Value.KindOneofCase.IntegerValue => value.IntegerValue,
        Value.KindOneofCase.BoolValue => value.BoolValue,
        _ => value.ToString()
    };
}
}
