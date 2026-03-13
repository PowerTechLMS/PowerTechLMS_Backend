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

    public MailBackgroundService(IMailQueue mailQueue, IEmailService emailService, ILogger<MailBackgroundService> logger)
    {
        _mailQueue = mailQueue;
        _emailService = emailService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mail Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await _mailQueue.DequeueAsync(stoppingToken);

                _logger.LogInformation("Processing email job for {To}, Attempt {Attempt}", job.To, job.RetryCount + 1);

                try
                {
                    await _emailService.SendEmailAsync(job.To, job.Subject, job.Body);
                    _logger.LogInformation("Successfully processed email job for {To}", job.To);
                }
                catch (Exception ex)
                {
                    var nextRetryCount = job.RetryCount + 1;
                    if (nextRetryCount < MaxRetries)
                    {
                        _logger.LogWarning(ex, "Failed to send email to {To}. Re-queueing for attempt {NextAttempt}.", job.To, nextRetryCount + 1);
                        
                        // Wait a bit before re-queueing to avoid immediate tight-loop failure
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, nextRetryCount)), stoppingToken);
                        
                        _mailQueue.Enqueue(job with { RetryCount = nextRetryCount });
                    }
                    else
                    {
                        _logger.LogCritical(ex, "Failed to send email to {To} after {MaxRetries} attempts. Giving up.", job.To, MaxRetries);
                        // No more re-queueing as per user request
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing mail background job.");
            }
        }

        _logger.LogInformation("Mail Background Service is stopping.");
    }
}
