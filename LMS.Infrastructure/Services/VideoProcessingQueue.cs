using System.Collections.Concurrent;

namespace LMS.Infrastructure.Services;

public interface IVideoProcessingQueue
{
    void Enqueue(int lessonId);

    Task<int> DequeueAsync(CancellationToken cancellationToken);
}

public class VideoProcessingQueue : IVideoProcessingQueue
{
    private readonly ConcurrentQueue<int> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Enqueue(int lessonId)
    {
        _queue.Enqueue(lessonId);
        _signal.Release();
    }

    public async Task<int> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var lessonId);
        return lessonId;
    }
}
