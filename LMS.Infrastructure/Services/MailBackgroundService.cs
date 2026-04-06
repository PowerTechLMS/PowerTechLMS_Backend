using LMS.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services;

public class MailBackgroundService : BackgroundService
{
    private readonly IMailQueue _mailQueue;
    private readonly IEmailService _emailService;
    private readonly ILogger<MailBackgroundService> _logger;
    private const int MaxRetries = 5;

    public MailBackgroundService(
        IMailQueue mailQueue,
        IEmailService emailService,
        ILogger<MailBackgroundService> logger)
    {
        _mailQueue = mailQueue;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _mailQueue.DequeueAsync(stoppingToken);

                try
                {
                    await _emailService.SendEmailAsync(job.To, job.Subject, job.Body);
                } catch (Exception ex)
                {
                    _logger.LogError(ex, "Background email job failed for {To}. Error: {Message}", job.To, ex.Message);
                    var nextRetryCount = job.RetryCount + 1;
                    if(nextRetryCount < MaxRetries)
                    {
                        var delay = (int)Math.Pow(2, nextRetryCount);
                        _logger.LogInformation("Retrying email to {To} in {Delay} seconds (Attempt {Count})", job.To, delay, nextRetryCount);
                        await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
                        _mailQueue.Enqueue(job with { RetryCount = nextRetryCount });
                    }
                }
            } catch(OperationCanceledException)
            {
            }
        }
    }
}
