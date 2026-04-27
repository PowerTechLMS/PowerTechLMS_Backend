using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LMS.Infrastructure.Services;

public class EssayService : IEssayService
{
    private readonly AppDbContext _db;
    private readonly ILlmService _llmService;
    private readonly INotificationService _notificationService;
    private readonly VectorDbService _vectorDb;
    private readonly ICertificateService _certificateService;

    public EssayService(
        AppDbContext db,
        ILlmService llmService,
        INotificationService notificationService,
        VectorDbService vectorDb,
        ICertificateService certificateService)
    {
        _db = db;
        _llmService = llmService;
        _notificationService = notificationService;
        _vectorDb = vectorDb;
        _certificateService = certificateService;
    }

    public async Task<EssayAttempt> StartAttemptAsync(int userId, int lessonId)
    {
        var lesson = await _db.Lessons.Include(l => l.EssayConfig).FirstOrDefaultAsync(l => l.Id == lessonId) ??
            throw new KeyNotFoundException("Không tìm thấy bài học.");

        if(lesson.EssayConfig is null)
            throw new InvalidOperationException("Bài học này không có cấu hình tự luận.");

        if(lesson.EssayConfig.MaxAttemptsPerWindow.HasValue && lesson.EssayConfig.AttemptWindowHours.HasValue)
        {
            var windowStart = DateTime.UtcNow.AddHours(-lesson.EssayConfig.AttemptWindowHours.Value);
            var attemptsInWindow = await _db.EssayAttempts
                .CountAsync(a => a.UserId == userId && a.LessonId == lessonId && a.CreatedAt >= windowStart);

            if(attemptsInWindow >= lesson.EssayConfig.MaxAttemptsPerWindow.Value)
                throw new InvalidOperationException("Bạn đã hết lượt làm bài trong khoảng thời gian này.");
        }

        var attempt = new EssayAttempt
        {
            UserId = userId,
            LessonId = lessonId,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow
        };

        _db.EssayAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        return attempt;
    }

    public async Task<EssayResultResponse> SubmitAttemptAsync(int userId, int attemptId, SubmitEssayRequest request)
    {
        var attempt = await _db.EssayAttempts
                .Include(a => a.Answers)
                .Include(a => a.Lesson)
                .ThenInclude(l => l.EssayConfig)
                .ThenInclude(c => c!.Questions)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên làm bài.");

        if(attempt.Status != "InProgress")
            throw new InvalidOperationException("Phiên làm bài này đã kết thúc.");

        if(attempt.Lesson.EssayConfig is null)
            throw new InvalidOperationException("Lỗi cấu hình bài học.");

        attempt.Status = "Submitted";
        attempt.SubmittedAt = DateTime.UtcNow;

        _db.EssayAnswers.RemoveRange(attempt.Answers);

        foreach(var answerRequest in request.Answers)
        {
            var answer = new EssayAnswer
            {
                AttemptId = attemptId,
                QuestionId = answerRequest.QuestionId,
                Content = answerRequest.Content
            };
            _db.EssayAnswers.Add(answer);
        }

        await _db.SaveChangesAsync();

        return await ScoreAttemptAsync(attemptId);
    }

    private async Task<EssayResultResponse> ScoreAttemptAsync(int attemptId)
    {
        var attempt = await _db.EssayAttempts
                .Include(a => a.Answers)
                .ThenInclude(ans => ans.Question)
                .Include(a => a.Lesson)
                .ThenInclude(l => l.EssayConfig)
                .FirstOrDefaultAsync(a => a.Id == attemptId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên làm bài để chấm điểm.");

        var questionsAndAnswers = attempt.Answers
            .Select(
                a => new
                {
                    QuestionId = a.QuestionId,
                    Question = a.Question.Content,
                    Criteria = a.Question.ScoringCriteria,
                    Answer = a.Content,
                    Weight = a.Question.Weight
                })
            .ToList();

        var promptBuilder = new StringBuilder();
        promptBuilder.AppendLine(
            "Bạn là một chuyên gia chấm bài tự luận. Hãy dựa vào danh sách câu hỏi và câu trả lời dưới đây để chấm điểm.");
        promptBuilder.AppendLine("CHÚ Ý: Hãy chấm điểm cho từng câu hỏi trên thang điểm 100.");
        promptBuilder.AppendLine("YÊU CẦU ĐẦU RA (JSON FORMAT):");
        promptBuilder.AppendLine("Trả về một đối tượng JSON có cấu trúc sau:");
        promptBuilder.AppendLine("{");
        promptBuilder.AppendLine("  \"aiFeedback\": \"Nhận xét tổng quan...\",");
        promptBuilder.AppendLine("  \"answers\": [");
        promptBuilder.AppendLine(
            "    { \"questionId\": 1, \"score\": 85, \"feedback\": \"Nhận xét chi tiết cho câu này (thang điểm 100)...\" }");
        promptBuilder.AppendLine("  ]");
        promptBuilder.AppendLine("}");
        promptBuilder.AppendLine("\nDANH SÁCH BÀI LÀM:");

        foreach(var qa in questionsAndAnswers)
        {
            promptBuilder.AppendLine($"[ID: {qa.QuestionId}] Câu hỏi: {qa.Question} (Tỉ trọng: {qa.Weight}%)");
            if(!string.IsNullOrEmpty(qa.Criteria))
            {
                promptBuilder.AppendLine($"Tiêu chí chấm điểm: {qa.Criteria}");
            }
            promptBuilder.AppendLine($"Câu trả lời của học viên: {qa.Answer}");
            promptBuilder.AppendLine("---");
        }

        promptBuilder.AppendLine(
            "\nYÊU CÀU: Trả về JSON đúng cấu trúc, trong đó 'questionId' PHẢI là ID thực tế (số nằm trong ngoặc [ID: ...]) của câu hỏi đó.");

        var aiResponse = await _llmService.GenerateResponseAsync(promptBuilder.ToString(), "Hãy chấm điểm bài thi này.");

        try
        {
            var jsonString = aiResponse.Trim();

            var match = Regex.Match(jsonString, @"```json\s*(.*?)\s*```", RegexOptions.Singleline);
            if(match.Success)
            {
                jsonString = match.Groups[1].Value;
            } else
            {
                match = Regex.Match(jsonString, @"```\s*(.*?)\s*```", RegexOptions.Singleline);
                if(match.Success)
                {
                    jsonString = match.Groups[1].Value;
                } else
                {
                    int firstBrace = jsonString.IndexOf('{');
                    int lastBrace = jsonString.LastIndexOf('}');
                    if(firstBrace >= 0 && lastBrace > firstBrace)
                    {
                        jsonString = jsonString.Substring(firstBrace, lastBrace - firstBrace + 1);
                    }
                }
            }

            jsonString = jsonString.Trim();

            using var doc = JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            attempt.AiFeedback = root.TryGetProperty("aiFeedback", out var feedbackProp)
                ? feedbackProp.GetString()
                : "Không có nhận xét tổng quan.";
            attempt.Status = "Completed";

            double calculatedTotalScore = 0;
            if(root.TryGetProperty("answers", out var answerScores))
            {
                foreach(var ansScore in answerScores.EnumerateArray())
                {
                    var qId = ansScore.GetProperty("questionId").GetInt32();
                    var answer = attempt.Answers.FirstOrDefault(a => a.QuestionId == qId);
                    if(answer is not null)
                    {
                        var score100 = ansScore.GetProperty("score").GetInt32();
                        answer.AiScore = score100;
                        answer.AiFeedback = ansScore.TryGetProperty("feedback", out var f)
                            ? f.GetString()
                            : string.Empty;

                        var weightPercent = (answer.Question?.Weight ?? 0) / 100.0;
                        calculatedTotalScore += (score100 * weightPercent);
                    }
                }
            }

            attempt.TotalScore = (int)Math.Round(calculatedTotalScore);
            attempt.IsPassed = attempt.TotalScore >= (attempt.Lesson.EssayConfig?.PassScore ?? 50);

            await _db.SaveChangesAsync();

            if(attempt.IsPassed)
            {
                await MarkLessonAsCompletedAsync(attempt.UserId, attempt.LessonId, true);
            }

            var attemptNumber = await _db.EssayAttempts
                .CountAsync(
                    a => a.UserId == attempt.UserId &&
                        a.LessonId == attempt.LessonId &&
                        a.StartedAt <= attempt.StartedAt);

            return new EssayResultResponse(
                attempt.Id,
                attemptNumber,
                attempt.TotalScore ?? 0,
                attempt.IsPassed,
                attempt.Lesson.EssayConfig?.PassScore ?? 50,
                attempt.Status,
                attempt.AiFeedback,
                attempt.Answers
                    .Select(
                        a => new EssayAnswerResultItem(
                                a.QuestionId,
                                a.Question.Content,
                                a.Content,
                                a.AiScore,
                                a.Question.Weight,
                                a.AiFeedback,
                                a.Question.ScoringCriteria))
                    .ToList(),
                attempt.ViolationCount);
        } catch(Exception ex)
        {
            attempt.AiFeedback = "Lỗi trong quá trình chấm điểm tự động: " + ex.Message;
            await _db.SaveChangesAsync();
            throw;
        }
    }

    public async Task<List<EssayAttemptSummary>> GetAttemptsByLessonAsync(
        int userId,
        int lessonId,
        bool isAdmin = false)
    {
        var attempts = await _db.EssayAttempts
            .Where(a => (a.UserId == userId || isAdmin) && a.LessonId == lessonId)
            .OrderBy(a => a.StartedAt)
            .ToListAsync();

        var count = 1;
        return attempts.Select(
            a => new EssayAttemptSummary(
                a.Id,
                count++,
                a.TotalScore,
                a.IsPassed,
                a.Status,
                a.StartedAt,
                a.SubmittedAt,
                a.ViolationCount))
            .OrderByDescending(a => a.StartedAt)
            .ToList();
    }

    public async Task<EssayResultResponse> GetAttemptDetailAsync(int userId, int attemptId, bool isAdmin = false)
    {
        var attempt = await _db.EssayAttempts
                .Include(a => a.Lesson)
                .ThenInclude(l => l.EssayConfig)
                .ThenInclude(c => c!.Questions)
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId && (a.UserId == userId || isAdmin || userId == 0)) ??
            throw new KeyNotFoundException("Không tìm thấy phiên làm bài.");

        var responseAnswers = attempt.Answers
            .GroupBy(a => a.QuestionId)
            .Select(g => g.OrderByDescending(x => x.AiScore.HasValue).First())
            .Select(
                a =>
                {
                    var question = attempt.Lesson.EssayConfig?.Questions.FirstOrDefault(q => q.Id == a.QuestionId);
                    return new EssayAnswerResultItem(
                        a.QuestionId,
                        question?.Content ?? string.Empty,
                        a.Content,
                        a.AiScore,
                        question?.Weight ?? 0,
                        a.AiFeedback,
                        question?.ScoringCriteria ?? string.Empty);
                })
            .ToList();

        var attemptNumber = await _db.EssayAttempts
            .CountAsync(
                a => a.UserId == attempt.UserId && a.LessonId == attempt.LessonId && a.StartedAt <= attempt.StartedAt);

        return new EssayResultResponse(
            attempt.Id,
            attemptNumber,
            attempt.TotalScore ?? 0,
            attempt.IsPassed,
            attempt.Lesson.EssayConfig?.PassScore ?? 50,
            attempt.Status,
            attempt.AiFeedback ?? string.Empty,
            responseAnswers,
            attempt.ViolationCount);
    }

    public async Task<EssayAttempt?> GetActiveAttemptAsync(int userId, int lessonId)
    {
        return await _db.EssayAttempts
            .Include(a => a.Answers)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.EssayConfig)
            .ThenInclude(c => c!.Questions)
            .OrderByDescending(a => a.StartedAt)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.LessonId == lessonId && a.Status == "InProgress");
    }

    public async Task SaveDraftAsync(int userId, int attemptId, SubmitEssayRequest request)
    {
        var attempt = await _db.EssayAttempts
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên làm bài.");

        if(attempt.Status != "InProgress")
            throw new InvalidOperationException("Phiên làm bài này đã kết thúc hoặc đã nộp.");

        _db.EssayAnswers.RemoveRange(attempt.Answers);

        foreach(var answerRequest in request.Answers)
        {
            var answer = new EssayAnswer
            {
                AttemptId = attemptId,
                QuestionId = answerRequest.QuestionId,
                Content = answerRequest.Content
            };
            _db.EssayAnswers.Add(answer);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<AdminEssayAttemptResponse>> GetAllAttemptsAsync()
    {
        var attempts = await _db.EssayAttempts
            .Include(a => a.User)
            .Include(a => a.Lesson)
            .ThenInclude(l => l.EssayConfig)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return attempts.Select(
            a => new AdminEssayAttemptResponse(
                a.Id,
                a.UserId,
                a.User.FullName,
                a.LessonId,
                a.Lesson.Title,
                a.Status,
                a.TotalScore,
                a.Lesson.EssayConfig?.PassScore ?? 50,
                a.IsPassed,
                a.AiFeedback,
                a.CreatedAt,
                new List<EssayAnswerResultItem>(),
                a.ViolationCount))
            .ToList();
    }

    public async Task UpdateAttemptAsync(int attemptId, AdminUpdateEssayAttemptRequest request)
    {
        var attempt = await _db.EssayAttempts
                .Include(a => a.Answers)
                .Include(a => a.Lesson)
                .ThenInclude(l => l.EssayConfig)
                .ThenInclude(c => c!.Questions)
                .FirstOrDefaultAsync(a => a.Id == attemptId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên làm bài.");

        decimal totalScore = 0;
        foreach(var ansReq in request.Answers)
        {
            var answer = attempt.Answers.FirstOrDefault(a => a.QuestionId == ansReq.QuestionId);
            if(answer != null)
            {
                answer.AiScore = ansReq.Score;
                answer.AiFeedback = ansReq.Feedback;

                var question = attempt.Lesson.EssayConfig?.Questions.FirstOrDefault(q => q.Id == ansReq.QuestionId);
                if(question != null)
                {
                    totalScore += (decimal)ansReq.Score * question.Weight / 100m;
                }
            }
        }

        attempt.TotalScore = (int)Math.Round(totalScore);
        attempt.AiFeedback = request.AiFeedback;
        attempt.IsPassed = attempt.TotalScore >= (attempt.Lesson.EssayConfig?.PassScore ?? 50);
        attempt.Status = "Completed";

        await _db.SaveChangesAsync();

        bool isAnyPassed = await _db.EssayAttempts
                .AnyAsync(
                    a => a.UserId == attempt.UserId &&
                            a.LessonId == attempt.LessonId &&
                            a.IsPassed &&
                            a.Id != attempt.Id) ||
            attempt.IsPassed;

        await MarkLessonAsCompletedAsync(attempt.UserId, attempt.LessonId, isAnyPassed);

        await _notificationService.CreateNotificationAsync(
            attempt.UserId,
            "Cập nhật kết quả bài tự luận",
            $"Giảng viên đã cập nhật kết quả bài tự luận '{attempt.Lesson.Title}'. Điểm: {attempt.TotalScore}. Kết quả: {(attempt.IsPassed ? "ĐẠT" : "CHƯA ĐẠT")}",
            "Essay");
    }

    public async Task<List<EssayQuestionDto>> GenerateQuestionsFromLessonsAsync(List<int> lessonIds)
    {
        if(lessonIds is null || !lessonIds.Any())
            return new List<EssayQuestionDto>();

        var searchQuery = "Hãy tạo các câu hỏi tự luận quan trọng và bao quát dựa trên nội dung bài học.";
        var searchResults = await _vectorDb.SearchAsync(searchQuery, lessonIds, limit: 10);

        var contentBuilder = new StringBuilder();
        foreach(var result in searchResults)
        {
            contentBuilder.AppendLine($"- {result.Content}");
        }

        var prompt = $@"Dựa vào nội dung kiến thức từ các bài học dưới đây, hãy tạo 3-5 câu hỏi tự luận để kiểm tra kiến thức học viên. 
Với mỗi câu hỏi, hãy cung cấp kèm theo ""Tiêu chí chấm điểm"" chi tiết để AI có thể dựa vào đó chấm điểm bài làm của học viên một cách chính xác nhất.

NỘI DUNG KIẾN THỨC:
{contentBuilder}

YÊU CẦU ĐẦU RA (JSON FORMAT):
Trả về một mảng JSON các đối tượng câu hỏi có cấu trúc:
[
  {{ ""content"": ""Nội dung câu hỏi..."", ""scoringCriteria"": ""Tiêu chí chấm điểm chi tiết cho câu hỏi này..."", ""sortOrder"": 1, ""weight"": 20 }}
]

LƯU Ý: Tổng ""weight"" của tất cả các câu hỏi phải luôn bằng 100. Trả lời thuần JSON, bằng tiếng Việt.";

        var aiResponse = await _llmService.GenerateResponseAsync(prompt, "Hãy tạo câu hỏi tự luận.");

        try
        {
            var jsonString = aiResponse.Trim();
            if(jsonString.StartsWith("```json"))
                jsonString = jsonString.Substring(7);
            if(jsonString.EndsWith("```"))
                jsonString = jsonString.Substring(0, jsonString.Length - 3);
            jsonString = jsonString.Trim();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<EssayQuestionDto>>(jsonString, options) ??
                new List<EssayQuestionDto>();
        } catch
        {
            return new List<EssayQuestionDto>();
        }
    }

    public async Task<int> GetAttemptNumberAsync(int userId, int lessonId, DateTime startedAt)
    {
        return await _db.EssayAttempts
            .CountAsync(a => a.UserId == userId && a.LessonId == lessonId && a.StartedAt <= startedAt);
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
        } else
        {
            var certificate = await _db.Certificates
                .FirstOrDefaultAsync(
                    c => c.UserId == userId && c.CourseId == lesson.Module.CourseId && c.Status == "Issued");

            if(certificate != null)
            {
                await _certificateService.RevokeCertificateAsync(
                    certificate.Id,
                    "Bài học trong khóa học bị hạ điểm dẫn đến không còn đủ điều kiện hoàn thành.",
                    1);
            }
        }
    }

    public async Task IncrementViolationCountAsync(int attemptId)
    {
        var attempt = await _db.EssayAttempts.FindAsync(attemptId);
        if(attempt is not null)
        {
            attempt.ViolationCount++;
            await _db.SaveChangesAsync();
        }
    }

    public async Task DeleteEssayQuestionAsync(int questionId)
    {
        var question = await _db.EssayQuestions.FindAsync(questionId) ??
            throw new KeyNotFoundException("Không tìm thấy câu hỏi tự luận.");
        question.IsDeleted = true;
        question.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
