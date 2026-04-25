using LMS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace LMS.Infrastructure.Services;

public class DocumentOutdatedJob
{
    private readonly IOutdatedDocumentScannerService _scanner;
    private readonly ILogger<DocumentOutdatedJob> _logger;

    public DocumentOutdatedJob(IOutdatedDocumentScannerService scanner, ILogger<DocumentOutdatedJob> logger)
    {
        _scanner = scanner;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("[Hangfire] Bắt đầu chạy Task định kỳ: Quét tài liệu lỗi thời.");
        await _scanner.ScanAllDocumentsAsync();
        _logger.LogInformation("[Hangfire] Hoàn tất Task định kỳ: Quét tài liệu lỗi thời.");
    }
}
