using Hangfire;
using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class RolePlayService : IRolePlayService
{
    private readonly AppDbContext _db;
    private readonly VectorDbService _vectorDb;
    private readonly ILlmService _llmService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly INotificationService _notificationService;
    private readonly ICertificateService _certificateService;

    public RolePlayService(
        AppDbContext db,
        VectorDbService vectorDb,
        ILlmService llmService,
        IBackgroundJobClient backgroundJobClient,
        INotificationService notificationService,
        ICertificateService certificateService)
    {
        _db = db;
        _vectorDb = vectorDb;
        _llmService = llmService;
        _backgroundJobClient = backgroundJobClient;
        _notificationService = notificationService;
        _certificateService = certificateService;
    }

    public async Task<RolePlaySession> StartSessionAsync(int userId, int lessonId)
    {
        var session = await _db.RolePlaySessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.LessonId == lessonId && s.Status == "InProgress");

        if(session is not null)
            return session;

        var config = await _db.RolePlayConfigs.FirstOrDefaultAsync(c => c.LessonId == lessonId);

        session = new RolePlaySession { UserId = userId, LessonId = lessonId, Status = "InProgress" };

        _db.RolePlaySessions.Add(session);
        await _db.SaveChangesAsync();

        var aiMessage = new RolePlayMessage
        {
            SessionId = session.Id,
            Role = "Ai",
            Content = config?.Scenario ?? "Chào bạn, chúng ta hãy bắt đầu phiên Role Play."
        };

        _db.RolePlayMessages.Add(aiMessage);
        await _db.SaveChangesAsync();

        return session;
    }

    public async Task<RolePlayMessage> SendMessageAsync(int userId, int sessionId, string content)
    {
        var session = await _db.RolePlaySessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên Role Play.");

        if(session.Status != "InProgress")
            throw new InvalidOperationException("Phiên Role Play này đã kết thúc.");

        var userMsg = new RolePlayMessage { SessionId = sessionId, Role = "User", Content = content };
        _db.RolePlayMessages.Add(userMsg);
        await _db.SaveChangesAsync();

        var history = session.Messages.OrderBy(m => m.CreatedAt).ToList();
        var historyText = string.Join("\n", history.Select(m => $"{m.Role}: {m.Content}"));

        var config = await _db.RolePlayConfigs.FirstOrDefaultAsync(c => c.LessonId == session.LessonId);
        var systemPrompt = BuildSystemPrompt(config);

        var aiResponseContent = await _llmService.GenerateResponseAsync(
            systemPrompt,
            $"Lịch sử cuộc trò chuyện:\n{historyText}\n\nNgười dùng vừa nói: {content}");

        var aiMsg = new RolePlayMessage { SessionId = sessionId, Role = "Ai", Content = aiResponseContent };
        _db.RolePlayMessages.Add(aiMsg);
        await _db.SaveChangesAsync();

        return aiMsg;
    }

    public async IAsyncEnumerable<string> SendMessageStreamingAsync(int userId, int sessionId, string content)
    {
        var session = await _db.RolePlaySessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên Role Play.");

        if(session.Status != "InProgress")
            throw new InvalidOperationException("Phiên Role Play này đã kết thúc.");

        var userMsg = new RolePlayMessage { SessionId = sessionId, Role = "User", Content = content };
        _db.RolePlayMessages.Add(userMsg);
        await _db.SaveChangesAsync();

        var history = session.Messages.OrderBy(m => m.CreatedAt).ToList();
        var historyText = string.Join("\n", history.Select(m => $"{m.Role}: {m.Content}"));
        var config = await _db.RolePlayConfigs.FirstOrDefaultAsync(c => c.LessonId == session.LessonId);
        var systemPrompt = BuildSystemPrompt(config);

        var fullContent = new StringBuilder();
        await foreach(var chunk in _llmService.GenerateResponseStreamingAsync(
            systemPrompt,
            $"Lịch sử cuộc trò chuyện:\n{historyText}\n\nNgười dùng vừa nói: {content}"))
        {
            fullContent.Append(chunk);
            yield return chunk;
        }

        var responseContent = fullContent.ToString();
        bool isFinishing = responseContent.Contains("[FINISH]");
        if(isFinishing)
        {
            responseContent = responseContent.Replace("[FINISH]", string.Empty).Trim();
            yield return "[DONE]";
        }

        var aiMsg = new RolePlayMessage { SessionId = sessionId, Role = "Ai", Content = responseContent };
        _db.RolePlayMessages.Add(aiMsg);
        await _db.SaveChangesAsync();

        if(isFinishing)
        {
            await FinishSessionAsync(userId, sessionId);
        }
    }

    private string BuildSystemPrompt(RolePlayConfig? config)
    {
        return $@"Bạn là một giảng viên chuyên gia đang thực hiện phiên Role Play để kiểm tra kiến thức của học viên.

TÌNH HUỐNG (SCENARIO):
{config?.Scenario}

MỤC TIÊU PHIÊN ROLE PLAY:
- Tiêu chí chấm điểm: {config?.ScoringCriteria}
- Yêu cầu bổ sung: {config?.AdditionalRequirements}

NHIỆM VỤ CỦA BẠN:
1. Đóng vai trong tình huống (scenario) đã được thiết lập ở trên.
2. Dẫn dắt học viên đi qua tình huống này để kiểm tra kiến thức của họ.
3. Nếu học viên trả lời sai hoặc chưa đủ ý, hãy khéo léo gợi ý hoặc hỏi xoáy vào điểm đó để kiểm tra chiều sâu sự hiểu biết.
4. LUÔN CHỦ ĐỘNG dẫn dắt cuộc hội thoại. Đừng chỉ trả lời 'Vâng' hay 'Đúng rồi', hãy hỏi tiếp hoặc đưa ra các thách thức tiếp theo trong tình huống.
5. KHI KẾT THÚC: Nếu bạn thấy mục tiêu của phiên Role Play đã đạt được hoặc cuộc hội thoại đã đi đến hồi kết tự nhiên, hãy thêm tag [FINISH] ở cuối cùng của câu trả lời.
6. Trả lời ngắn gọn, súc tích, tự nhiên bằng tiếng Việt.";
    }

    public async Task<RolePlaySuggestionResponse> GenerateScenarioFromLessonsAsync(List<int> lessonIds)
    {
        if(lessonIds is null || !lessonIds.Any())
            return new RolePlaySuggestionResponse(string.Empty, string.Empty, string.Empty);

        var searchQuery = "Hãy tạo một tình huống Role Play thực tế và chuyên nghiệp dựa trên nội dung bài học.";
        var searchResults = await _vectorDb.SearchAsync(searchQuery, lessonIds, limit: 12);

        var contentBuilder = new StringBuilder();
        if(searchResults.Any())
        {
            foreach(var result in searchResults)
            {
                contentBuilder.AppendLine($"- {result.Content}");
            }
        } else
        {
            var lessons = await _db.Lessons.Where(l => lessonIds.Contains(l.Id)).ToListAsync();
            foreach(var lesson in lessons)
            {
                contentBuilder.AppendLine($"### Bài học: {lesson.Title}");
                var content = lesson.Content ?? string.Empty;
                contentBuilder.AppendLine(content.Length > 2000 ? content.Substring(0, 2000) + "..." : content);
            }
        }

        var prompt = $@"Dựa vào nội dung kiến thức từ các bài học dưới đây, hãy thiết lập một cấu hình hoàn chỉnh cho phiên Role Play.

NỘI DUNG KIẾN THỨC TRỌNG TÂM:
{contentBuilder}

YÊU CẦU ĐẦU RA (JSON FORMAT):
Hãy trả về một đối tượng JSON duy nhất có cấu trúc sau:
{{
  ""scenario"": ""Nội dung tình huống chi tiết, thực tế, xác định rõ vai của AI và học viên. Kết thúc bằng lời chào/câu hỏi mở đầu."",
  ""scoringCriteria"": ""Các tiêu chí chấm điểm cụ thể (ví dụ: Kỹ năng chào hỏi: 2đ, Giải quyết vấn đề: 4đ...)"",
  ""additionalRequirements"": ""Các yêu cầu về thái độ của AI (ví dụ: AI đóng vai khách hàng khó tính, hay bắt bẻ...)""
}}

LƯU Ý: 
1. Trả lời thuần JSON, không kèm giải thích.
2. Tất cả nội dung bằng tiếng Việt.";

        var aiResponse = await _llmService.GenerateResponseAsync(prompt, "Hãy tạo cấu hình Role Play dưới dạng JSON.");

        try
        {
            var jsonString = aiResponse.Trim();
            if(jsonString.StartsWith("```json"))
                jsonString = jsonString.Substring(7);
            if(jsonString.EndsWith("```"))
                jsonString = jsonString.Substring(0, jsonString.Length - 3);
            jsonString = jsonString.Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<RolePlaySuggestionResponse>(jsonString, options);

            return result ?? new RolePlaySuggestionResponse(aiResponse, string.Empty, string.Empty);
        } catch
        {
            return new RolePlaySuggestionResponse(aiResponse, string.Empty, string.Empty);
        }
    }

    public async Task FinishSessionAsync(int userId, int sessionId)
    {
        var session = await _db.RolePlaySessions.FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên Role Play.");

        if(session.Status != "InProgress")
            return;

        session.Status = "Finished";
        await _db.SaveChangesAsync();

        _backgroundJobClient.Enqueue<RolePlayScoringJob>(job => job.ScoreSessionAsync(session.Id));
    }

    public async Task<RolePlaySession?> GetSessionAsync(int userId, int lessonId)
    {
        return await _db.RolePlaySessions
            .Include(s => s.Messages)
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.LessonId == lessonId);
    }

    public async Task<List<RolePlaySession>> GetUserSessionsAsync(int userId)
    {
        return await _db.RolePlaySessions
            .Include(s => s.Lesson)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<RolePlaySession>> GetSessionsByLessonAsync(int userId, int lessonId)
    {
        return await _db.RolePlaySessions
            .Where(s => s.UserId == userId && s.LessonId == lessonId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<RolePlaySession?> GetSessionByIdAsync(int userId, int sessionId)
    {
        return await _db.RolePlaySessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task<List<RolePlaySessionResponse>> GetAllSessionsAsync()
    {
        var sessions = await _db.RolePlaySessions
            .Include(s => s.User)
            .Include(s => s.Lesson)
            .Include(s => s.Messages)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        var lessonIds = sessions.Select(s => s.LessonId).Distinct().ToList();
        var configs = await _db.RolePlayConfigs
            .Where(c => lessonIds.Contains(c.LessonId))
            .ToDictionaryAsync(c => c.LessonId, c => c.PassScore);

        return sessions.Select(
            s => new RolePlaySessionResponse(
                s.Id,
                s.UserId,
                s.User?.FullName,
                s.LessonId,
                s.Lesson?.Title,
                s.Status,
                s.Score,
                configs.ContainsKey(s.LessonId) ? configs[s.LessonId] : 50,
                s.Feedback,
                s.CreatedAt,
                s.Messages.Select(m => new RolePlayMessageResponse(m.Id, m.Role, m.Content, m.CreatedAt)).ToList(),
                s.ViolationCount))
            .ToList();
    }

    public async Task UpdateSessionStatusAsync(int sessionId, string status, int? score, string? feedback)
    {
        var session = await _db.RolePlaySessions
                .Include(s => s.User)
                .Include(s => s.Lesson)
                .FirstOrDefaultAsync(s => s.Id == sessionId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên Role Play.");

        var oldStatus = session.Status;
        session.Status = status;
        session.Score = score;
        session.Feedback = feedback;

        await _db.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            session.UserId,
            "Cập nhật kết quả Role Play",
            $"Kết quả phiên Role Play bài '{session.Lesson.Title}' của bạn đã được cập nhật thành: {status}. Điểm: {score ?? 0}",
            "RolePlay");

        var config = await _db.RolePlayConfigs.FirstOrDefaultAsync(c => c.LessonId == session.LessonId);
        int passScore = config?.PassScore ?? 50;

        bool isAnyPassed = await _db.RolePlaySessions
                .AnyAsync(
                    s => s.UserId == session.UserId &&
                            s.LessonId == session.LessonId &&
                            s.Status == "Scored" &&
                            s.Score >= passScore &&
                            s.Id != session.Id) ||
            (status == "Scored" && score >= passScore) ||
            status == "Completed";

        await MarkLessonAsCompletedAsync(session.UserId, session.LessonId, isAnyPassed);
    }

    private async Task MarkLessonAsCompletedAsync(int userId, int lessonId, bool isCompleted)
    {
        var lesson = await _db.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);

        if(lesson == null)
            return;

        var progress = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if(progress == null && isCompleted)
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
        } else if(progress != null)
        {
            progress.IsCompleted = isCompleted;
            progress.CompletedAt = isCompleted ? DateTime.UtcNow : null;
            progress.WatchedPercent = isCompleted ? 100 : 0;
        }

        await _db.SaveChangesAsync();

        if(isCompleted)
        {
            await _certificateService.IssueCertificateAsync(userId, lesson.Module.CourseId);
        }
    }

    public async Task IncrementViolationCountAsync(int sessionId)
    {
        var session = await _db.RolePlaySessions.FindAsync(sessionId);
        if(session is not null)
        {
            session.ViolationCount++;
            await _db.SaveChangesAsync();
        }
    }
}
