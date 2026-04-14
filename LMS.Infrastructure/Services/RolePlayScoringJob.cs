using LMS.Core.Interfaces;
using LMS.Core.Entities;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class RolePlayScoringJob
{
    private readonly AppDbContext _db;
    private readonly ILlmService _llmService;
    private readonly ICertificateService _certificateService;
    private readonly VectorDbService _vectorDb;
    private readonly ILogger<RolePlayScoringJob> _logger;

    public RolePlayScoringJob(
        AppDbContext db,
        ILlmService llmService,
        VectorDbService vectorDb,
        ICertificateService certificateService,
        ILogger<RolePlayScoringJob> logger)
    {
        _db = db;
        _llmService = llmService;
        _vectorDb = vectorDb;
        _certificateService = certificateService;
        _logger = logger;
    }

    public async Task ScoreSessionAsync(int sessionId)
    {
        _logger.LogInformation("[RolePlay] Đang chấm điểm phiên: {SessionId}", sessionId);

        var session = await _db.RolePlaySessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .Include(s => s.Lesson)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null) return;

        var config = await _db.RolePlayConfigs.FirstOrDefaultAsync(c => c.LessonId == session.LessonId);
        
        // 1. Prepare conversation text
        var conversation = string.Join("\n", session.Messages.Select(m => $"{m.Role}: {m.Content}"));

        // 2. Get Ground Truth context from Vector DB for scoring
        var supportLessonIds = string.IsNullOrEmpty(config?.SupportLessonIds)
            ? new List<int>()
            : JsonSerializer.Deserialize<List<int>>(config.SupportLessonIds) ?? new List<int>();

        string groundTruth = string.Empty;
        if (supportLessonIds.Any())
        {
            var searchResults = await _vectorDb.SearchAsync("Nội dung quan trọng và kiến thức cốt lõi cần ghi nhớ", supportLessonIds, limit: 10);
            groundTruth = string.Join("\n", searchResults.Select(r => r.Content));
        }

        // 3. Ask AI to score
        var criteria = config?.ScoringCriteria ?? "Hãy chấm điểm dựa trên mức độ chuyên nghiệp và kiến thức thể hiện trong cuộc đối thoại.";
        
        var prompt = $@"Bạn là một giảng viên chuyên gia đang chấm điểm bài thực hành Role Play của học viên.

KIẾN THỨC CHUẨN (Ground Truth) để đối chiếu:
{groundTruth}

TIÊU CHÍ CHẤM ĐIỂM:
{criteria}

YÊU CẦU BỔ SUNG:
{config?.AdditionalRequirements}

NỘI DUNG CUỘC ĐỐI THOẠI CẦN CHẤM ĐIỂM:
{conversation}

NHIỆM VỤ CỦA BẠN:
1. Đánh giá xem học viên có nắm vững KIẾN THỨC CHUẨN hay không.
2. Kiểm tra mức độ xử lý tình huống và thái độ chuyên nghiệp.
3. Cung cấp điểm số (score) và nhận xét (feedback) chi tiết.

Hãy trả về kết quả dưới dạng JSON có cấu trúc như sau:
{{
  ""score"": 85,
  ""feedback"": ""Bạn đã làm rất tốt việc áp dụng kiến thức A, tuy nhiên cần chú ý hơn về...""
}}
Thay đổi score thành số từ 0 đến 100.
CHỈ TRẢ VỀ JSON, KHÔNG CÓ GIẢI THÍCH THÊM.";

        try
        {
            var response = await _llmService.GenerateResponseAsync(prompt, "Hãy chấm điểm phiên Role Play này.");
            
            // Clean JSON response (sometimes LLM adds markdown blocks)
            var json = response.Trim();
            if (json.StartsWith("```json")) json = json.Substring(7);
            if (json.EndsWith("```")) json = json.Substring(0, json.Length - 3);
            json = json.Trim();

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            int score = root.GetProperty("score").GetInt32();
            string feedback = root.GetProperty("feedback").GetString() ?? string.Empty;

            session.Score = score;
            session.Feedback = feedback;
            session.Status = "Completed";

            // 3. Mark Lesson as Completed if score >= 80 (hoặc tiêu chí nào đó)
            // Giả sử 80 là điểm đậu.
            int passScore = config?.PassScore ?? 50;
            if (session.Score >= passScore)
            {
                await MarkLessonAsCompletedAsync(session.UserId, session.LessonId);
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("[RolePlay] Đã chấm điểm xong phiên {SessionId}: {Score}. Pass: {IsPass} (Yêu cầu: {PassScore})", sessionId, session.Score, session.Score >= passScore, passScore);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RolePlay] Lỗi khi chấm điểm phiên {SessionId}", sessionId);
            session.Status = "ScoringFailed";
            await _db.SaveChangesAsync();
        }
    }

    private async Task MarkLessonAsCompletedAsync(int userId, int lessonId)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null) return;

        var progress = await _db.LessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                WatchedPercent = 100
            };
            _db.LessonProgresses.Add(progress);
        }
        else if (!progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
            progress.WatchedPercent = 100;
        }

        await _db.SaveChangesAsync();

        // Trigger certificate check - Must pass CourseId
        await _certificateService.IssueCertificateAsync(userId, lesson.Module.CourseId);
    }
}
