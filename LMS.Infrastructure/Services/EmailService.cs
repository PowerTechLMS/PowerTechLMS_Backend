using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace LMS.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IMailQueue _mailQueue;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IMailQueue mailQueue, IConfiguration config, ILogger<EmailService> logger)
    {
        _mailQueue = mailQueue;
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpHost = _config["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPortStr = _config["EmailSettings:SmtpPort"] ?? "587";
            var smtpUser = _config["EmailSettings:SmtpUser"];
            var smtpPass = _config["EmailSettings:SmtpPass"];
            var fromEmail = _config["EmailSettings:FromEmail"] ?? smtpUser;

            if(string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass) || smtpUser.Contains("your-email"))
            {
                _logger.LogWarning("Email sending skipped: SMTP credentials are not configured in appsettings.json");
                return;
            }

            if(!int.TryParse(smtpPortStr, out int smtpPort))
                smtpPort = 587;

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail!, "PowerTech LMS"),
                Subject = subject,
                Body = GetEmailTemplate(body),
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            _logger.LogInformation("Attempting to send email to {To} via {Host}", to, smtpHost);
            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {To}", to);
        } catch(Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending email to {To}", to);
            throw;
        }
    }

    public void QueueEmail(string to, string subject, string body)
    { _mailQueue.Enqueue(new MailJob(to, subject, body, 0)); }

    private string GetEmailTemplate(string content)
    {
        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                .container {{ max-width: 600px; margin: 20px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
                .header {{ background: linear-gradient(135deg, #1e3a8a, #3b82f6); color: #ffffff; padding: 30px; text-align: center; }}
                .content {{ padding: 30px; line-height: 1.6; color: #333333; }}
                .footer {{ background-color: #f9fafb; padding: 20px; text-align: center; font-size: 12px; color: #6b7280; border-top: 1px solid #e5e7eb; }}
                .button {{ display: inline-block; padding: 12px 24px; background-color: #3b82f6; color: #ffffff; text-decoration: none; border-radius: 6px; margin-top: 20px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>PowerTech LMS</h1>
                </div>
                <div class='content'>
                    {content}
                </div>
                <div class='footer'>
                    &copy; {DateTime.Now.Year} PowerTech. All rights reserved.<br>
                    Đây là email tự động, vui lòng không phản hồi.
                </div>
            </div>
        </body>
        </html>";
    }
}
