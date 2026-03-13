using System.Threading.Channels;
using LMS.Core.DTOs;
using LMS.Core.Interfaces;

namespace LMS.Infrastructure.Services;

public class MailQueue : IMailQueue
{
    private readonly Channel<MailJob> _queue;

    public MailQueue()
    {
        // Unbounded channel for simplicity, can be bounded for backpressure
        _queue = Channel.CreateUnbounded<MailJob>();
    }

    public void Enqueue(MailJob job)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        _queue.Writer.TryWrite(job);
    }

    public async Task<MailJob> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}
