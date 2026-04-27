using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class OutdatedDocumentScannerService : IOutdatedDocumentScannerService
{
    private readonly AppDbContext _db;
    private readonly ILlmService _llm;
    private readonly IEmailService _email;
    private readonly VectorDbService _vectorDb;
    private readonly ILogger<OutdatedDocumentScannerService> _logger;
    private readonly IConfiguration _config;

    public OutdatedDocumentScannerService(
        AppDbContext db,
        ILlmService llm,
        IEmailService email,
        VectorDbService vectorDb,
        ILogger<OutdatedDocumentScannerService> logger,
        IConfiguration config)
    {
        _db = db;
        _llm = llm;
        _email = email;
        _vectorDb = vectorDb;
        _logger = logger;
        _config = config;
    }

    public async Task ScanAllDocumentsAsync()
    {
        _logger.LogInformation("[OutdatedScanner] Bắt đầu quét tài liệu lỗi thời bằng Vector DB...");

        var documents = await _db.Documents.Where(d => !d.IsDeleted).ToListAsync();

        var now = DateTime.UtcNow;
        var documentsToNotify = new List<Document>();

        foreach(var doc in documents)
        {
            try
            {
                var searchResults = await _vectorDb.SearchByDocumentAsync(
                    "thời gian, ngày tháng, năm, thời hạn, cập nhật, 2022, 2023, 2024, quy trình cũ, quy định cũ",
                    doc.Id,
                    limit: 10);

                if(!searchResults.Any())
                {
                    _logger.LogInformation(
                        "[OutdatedScanner] Tài liệu {Id} chưa được xử lý AI hoặc không có nội dung text.",
                        doc.Id);
                    continue;
                }

                var relevantContent = string.Join("\n---\n", searchResults.Select(r => r.Content));
                var scanResult = await AnalyzeOutdatedSignsWithAiAsync(doc.Title, relevantContent);

                if(scanResult.IsOutdated)
                {
                    doc.OutdatedReason = scanResult.Reason;
                    if(!doc.IsOutdated)
                    {
                        doc.IsOutdated = true;
                        doc.OutdatedAt = now;
                        documentsToNotify.Add(doc);
                    } else
                    {
                        if(doc.LastOutdatedNotifiedAt is null || (now - doc.LastOutdatedNotifiedAt.Value).TotalDays >= 7)
                        {
                            documentsToNotify.Add(doc);
                        }
                    }
                } else
                {
                    doc.IsOutdated = false;
                    doc.OutdatedAt = null;
                    doc.LastOutdatedNotifiedAt = null;
                    doc.OutdatedReason = null;
                }

                await _db.SaveChangesAsync();
            } catch(Exception ex)
            {
                _logger.LogError(ex, "[OutdatedScanner] Lỗi khi quét tài liệu {Id}", doc.Id);
            }
        }

        if(documentsToNotify.Any())
        {
            await NotifyAllOutdatedDocumentsAsync(documentsToNotify);
            foreach(var doc in documentsToNotify)
            {
                doc.LastOutdatedNotifiedAt = now;
            }
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("[OutdatedScanner] Hoàn tất quét tài liệu lỗi thời.");
    }

    private async Task<OutdatedCheckResult> AnalyzeOutdatedSignsWithAiAsync(string title, string snippets)
    {
        var prompt = $@"
Bạn là một chuyên gia quản lý chất lượng tài liệu. Dưới đây là một số đoạn trích từ tài liệu '{title}'.
Nhiệm vụ của bạn là xác định xem tài liệu này có dấu hiệu bị lỗi thời (outdated) dựa trên các đoạn trích này hay không.
Thời điểm hiện tại là: {DateTime.Now:MM/yyyy}.

Một tài liệu bị coi là lỗi thời nếu:
1. Có thông tin ngày tháng, năm cũ (ví dụ: kế hoạch năm 2023, nội quy 2022).
2. Đề cập đến các quy trình/công nghệ đã cũ.
3. Có chỉ định cụ thể về thời hạn đã qua.

Hãy trả về kết quả định dạng JSON:
{{
  ""isOutdated"": true/false,
  ""reason"": ""Nếu true, hãy trích dẫn bằng chứng và giải thích lý do tại sao lỗi thời. Nếu false, để trống.""
}}

Đoạn trích tài liệu:
{snippets}
";

        try
        {
            var response = await _llm.GenerateResponseAsync(prompt, "Bạn là chuyên gia phân tích tài liệu.");
            var startIndex = response.IndexOf('{');
            var endIndex = response.LastIndexOf('}');
            if(startIndex >= 0 && endIndex > startIndex)
            {
                var jsonStr = response.Substring(startIndex, endIndex - startIndex + 1);
                return JsonSerializer.Deserialize<OutdatedCheckResult>(
                        jsonStr,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
                    new OutdatedCheckResult();
            }
        } catch(Exception ex)
        {
            _logger.LogError(ex, "[OutdatedScanner] Lỗi AI phân tích tài liệu {Title}", title);
        }

        return new OutdatedCheckResult();
    }

    private async Task NotifyAllOutdatedDocumentsAsync(List<Document> documents)
    {
        var users = await _db.Users
            .Where(u => u.IsActive && !u.IsDeleted)
            .Where(u => u.UserRoles.Any(ur => ur.Role.RolePermissions.Any(rp => rp.Permission.Code == "doc.upload")))
            .ToListAsync();

        if(!users.Any())
            return;

        var subject = $"[CẢNH BÁO TỔNG HỢP] {documents.Count} tài liệu có dấu hiệu lỗi thời";
        var sb = new StringBuilder();
        sb.Append("<p>Xin chào,</p>");
        sb.Append(
            $"<p>Hệ thống AI đã phát hiện <strong>{documents.Count}</strong> tài liệu có dấu hiệu lỗi thời hoặc cần cập nhật mới:</p>");

        sb.Append("<table style='width:100%; border-collapse: collapse; margin-top: 20px;'>");
        sb.Append("<tr style='background-color: #f8f9fa;'>");
        sb.Append("<th style='border: 1px solid #dee2e6; padding: 12px; text-align: left;'>Tên tài liệu</th>");
        sb.Append("<th style='border: 1px solid #dee2e6; padding: 12px; text-align: left;'>Bằng chứng / Lý do</th>");
        sb.Append("</tr>");

        foreach(var doc in documents)
        {
            sb.Append("<tr>");
            sb.Append($"<td style='border: 1px solid #dee2e6; padding: 12px;'><strong>{doc.Title}</strong></td>");
            sb.Append(
                $"<td style='border: 1px solid #dee2e6; padding: 12px; color: #d63384;'>{doc.OutdatedReason}</td>");
            sb.Append("</tr>");
        }
        sb.Append("</table>");

        sb.Append(
            "<p style='margin-top: 25px;'>Vui lòng kiểm tra lại tính chính xác và cập nhật phiên bản mới nếu cần thiết.</p>");
        sb.Append(
            $"<p><a href='{_config["FrontendUrl"]}/documents' style='display: inline-block; padding: 10px 20px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>Quản lý tài liệu</a></p>");

        foreach(var user in users)
        {
            _email.QueueEmail(user.Email, subject, sb.ToString());
        }

        _logger.LogInformation(
            "[OutdatedScanner] Đã gửi email tổng hợp cho {Count} tài liệu tới {UserCount} người dùng.",
            documents.Count,
            users.Count);
    }

    private class OutdatedCheckResult
    {
        public bool IsOutdated { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}
