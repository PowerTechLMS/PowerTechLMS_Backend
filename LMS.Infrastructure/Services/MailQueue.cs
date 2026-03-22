using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using System.Threading.Channels;

namespace LMS.Infrastructure.Services;

public class MailQueue : IMailQueue
{
    private readonly Channel<MailJob> _queue;

    public MailQueue() { _queue = Channel.CreateUnbounded<MailJob>(); }

    public void Enqueue(MailJob job)
    {
        if(job == null)
            throw new ArgumentNullException(nameof(job));
        _queue.Writer.TryWrite(job);
    }

    public async Task<MailJob> DequeueAsync(CancellationToken cancellationToken)
    { return await _queue.Reader.ReadAsync(cancellationToken); }
}
