using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class QuizService : IQuizService
{
    private readonly AppDbContext _db;
    private readonly IProgressService _progressService;
    private readonly ICertificateService _certificateService;

    public QuizService(AppDbContext db, IProgressService progressService, ICertificateService certificateService)
    {
        _db = db;
        _progressService = progressService;
        _certificateService = certificateService;
    }

    public async Task<QuizDetailResponse?> GetQuizDetailAsync(int quizId)
    {
        return await _db.Quizzes
            .Where(q => q.Id == quizId && !q.IsDeleted)
            .Select(
                q => new QuizDetailResponse(
                    q.Id,
                    q.Title,
                    q.TimeLimitMinutes,
                    q.PassScore,
                    q.QuestionCount,
                    _db.QuestionBanks
                        .Where(qb => qb.QuizId == q.Id)
                        .Select(
                            qb => new QuestionBankResponse(
                                        qb.Id,
                                        qb.QuestionText,
                                        qb.OptionA,
                                        qb.OptionB,
                                        qb.OptionC,
                                        qb.OptionD,
                                        qb.CorrectAnswer,
                                        qb.Points,
                                        qb.Explanation))
                        .ToList(),
                    q.RetakeWaitTimeMinutes,
                    q.MaxRetakesPerDay))
            .FirstOrDefaultAsync();
    }

    public async Task<Quiz> CreateQuizAsync(int courseId, CreateQuizRequest request)
    {
        var existingFinalQuizzes = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .ToListAsync();

        foreach(var eq in existingFinalQuizzes)
        {
            eq.IsDeleted = true;
            eq.DeletedAt = DateTime.UtcNow;
        }

        var quiz = new Quiz
        {
            CourseId = courseId,
            Title = request.Title,
            TimeLimitMinutes = request.TimeLimitMinutes,
            PassScore = 5,
            QuestionCount = request.QuestionCount,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleAnswers = request.ShuffleAnswers,
            RetakeWaitTimeMinutes = request.RetakeWaitTimeMinutes,
            MaxRetakesPerDay = request.MaxRetakesPerDay,
            CreatedAt = DateTime.UtcNow
        };

        _db.Quizzes.Add(quiz);
        await _db.SaveChangesAsync();
        return quiz;
    }

    public async Task<QuestionResponse> AddQuestionAsync(int quizId, CreateQuestionRequest request)
    {
        var question = new QuestionBank
        {
            QuizId = quizId,
            QuestionText = string.IsNullOrWhiteSpace(request.Content) ? "Nội dung câu hỏi" : request.Content,
            OptionA = request.OptionA ?? string.Empty,
            OptionB = request.OptionB ?? string.Empty,
            OptionC = request.OptionC ?? string.Empty,
            OptionD = request.OptionD ?? string.Empty,
            CorrectAnswer = string.IsNullOrWhiteSpace(request.CorrectAnswer) ? "A" : request.CorrectAnswer,
            Points = 1.0m,
            Explanation = request.Explanation,
            CreatedAt = DateTime.UtcNow
        };

        _db.QuestionBanks.Add(question);
        await _db.SaveChangesAsync();

        return new QuestionResponse(
            question.Id,
            question.QuestionText,
            question.OptionA,
            question.OptionB,
            question.OptionC,
            question.OptionD,
            question.CorrectAnswer,
            (double)question.Points);
    }

    public async Task<QuizResultResponse> SubmitQuizAsync(int userId, int attemptId, SubmitQuizRequest request)
    {
        var attempt = await _db.QuizAttempts
                .Include(a => a.Quiz)
                .ThenInclude(q => q.Course)
                .Include(a => a.Answers)
                .ThenInclude(a => a.Question)
                .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId) ??
            throw new KeyNotFoundException("Không tìm thấy phiên thi.");

        if(attempt.Status != "InProgress")
            throw new InvalidOperationException("Bài thi đã kết thúc.");

        var correctCount = 0;
        var totalQuestions = attempt.Answers.Count;
        var details = new List<QuizAnswerDetailResponse>();

        foreach(var submitted in request.Answers)
        {
            var ans = attempt.Answers.FirstOrDefault(a => a.QuestionId == submitted.QuestionId);
            if(ans == null)
                continue;

            var isCorrect = ans.Question.CorrectAnswer
                .Equals(submitted.SelectedAnswer, StringComparison.OrdinalIgnoreCase);
            ans.SelectedAnswer = submitted.SelectedAnswer;
            ans.IsCorrect = isCorrect;
            ans.AnsweredAt = DateTime.UtcNow;

            if(isCorrect)
                correctCount++;

            details.Add(
                new QuizAnswerDetailResponse(
                    ans.QuestionId,
                    ans.Question.QuestionText,
                    submitted.SelectedAnswer,
                    ans.Question.CorrectAnswer,
                    isCorrect,
                    ans.Question.Explanation));
        }

        decimal rawScore = totalQuestions > 0 ? (decimal)correctCount / totalQuestions * 10 : 0;
        decimal finalScore = Math.Round(rawScore, 1);

        attempt.Score = finalScore;
        attempt.IsPassed = finalScore >= (decimal)attempt.Quiz.PassScore;
        attempt.Status = "Submitted";
        attempt.SubmittedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if(attempt.IsPassed)
        {
            var progress = await _progressService.GetCourseProgressAsync(userId, attempt.Quiz.CourseId);
            if(progress.IsCompleted)
            {
                await _certificateService.IssueCertificateAsync(userId, attempt.Quiz.CourseId);

                var enrollment = await _db.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == attempt.Quiz.CourseId);
                if(enrollment != null)
                {
                    enrollment.Status = "Completed";
                    await _db.SaveChangesAsync();
                }
            }
        }

        var startOfToday = DateTime.UtcNow.Date;
        var attemptsDoneToday = await _db.QuizAttempts
            .CountAsync(
                a => a.UserId == userId &&
                    a.QuizId == attempt.QuizId &&
                    a.StartedAt >= startOfToday &&
                    a.Status == "Submitted");
        int maxAllowed = attempt.Quiz.MaxRetakesPerDay ?? attempt.Quiz.Course.QuizMaxRetakesPerDay;
        int remainingAttempts = Math.Max(0, maxAllowed - attemptsDoneToday);
        int waitMinutes = attempt.Quiz.RetakeWaitTimeMinutes ?? attempt.Quiz.Course.QuizRetakeWaitTimeMinutes;

        return new QuizResultResponse(
            attemptId,
            finalScore,
            correctCount,
            totalQuestions,
            attempt.IsPassed,
            details,
            remainingAttempts,
            waitMinutes);
    }

    public async Task<StartQuizResponse> StartQuizAsync(int userId, int quizId)
    {
        var quiz = await _db.Quizzes
                .Include(q => q.Questions)
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == quizId && !q.IsDeleted) ??
            throw new KeyNotFoundException("Không tìm thấy bài thi.");

        var existingAttempt = await _db.QuizAttempts
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.QuizId == quizId && a.Status == "InProgress");

        if(existingAttempt != null)
        {
            var draftAnswers = existingAttempt.Answers
                .Where(a => !string.IsNullOrEmpty(a.SelectedAnswer))
                .ToDictionary(a => a.QuestionId, a => a.SelectedAnswer!);
            var restoredQuestions = existingAttempt.Answers
                .Select(
                    a => new QuizQuestionResponse(
                        a.QuestionId,
                        a.Question.QuestionText,
                        a.Question.OptionA,
                        a.Question.OptionB,
                        a.Question.OptionC,
                        a.Question.OptionD))
                .ToList();

            var t = DateTime.UtcNow.Date;
            var doneToday = await _db.QuizAttempts
                .CountAsync(
                    a => a.UserId == userId && a.QuizId == quizId && a.StartedAt >= t && a.Status == "Submitted");
            int max = quiz.MaxRetakesPerDay ?? quiz.Course.QuizMaxRetakesPerDay;
            int remaining = Math.Max(0, max - doneToday);

            return new StartQuizResponse(
                existingAttempt.Id,
                quiz.TimeLimitMinutes,
                existingAttempt.StartedAt,
                restoredQuestions,
                draftAnswers,
                existingAttempt.RemainingSeconds,
                remaining);
        }

        var today = DateTime.UtcNow.Date;
        var finishedAttemptsToday = await _db.QuizAttempts
            .Where(a => a.UserId == userId && a.QuizId == quizId && a.StartedAt >= today && a.Status == "Submitted")
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();

        int maxRetakes = quiz.MaxRetakesPerDay ?? quiz.Course.QuizMaxRetakesPerDay;
        int remainingAttempts = maxRetakes - finishedAttemptsToday.Count;

        if(remainingAttempts <= 0)
        {
            throw new InvalidOperationException(
                $"Bạn đã hết lượt làm bài trong ngày (Tối đa {maxRetakes} lần/ngày). Vui lòng quay lại vào ngày mai.");
        }

        var lastAttempt = finishedAttemptsToday.FirstOrDefault();
        if(lastAttempt != null && !lastAttempt.IsPassed && lastAttempt.SubmittedAt.HasValue)
        {
            int waitMinutes = quiz.RetakeWaitTimeMinutes ?? quiz.Course.QuizRetakeWaitTimeMinutes;
            var timePassed = DateTime.UtcNow - lastAttempt.SubmittedAt.Value;
            if(timePassed.TotalMinutes < waitMinutes)
            {
                var remainingMinutes = Math.Ceiling(waitMinutes - timePassed.TotalMinutes);
                throw new InvalidOperationException($"Vui lòng ôn tập thêm và quay lại sau {remainingMinutes} phút.");
            }
        }

        var questions = quiz.Questions.Take(quiz.QuestionCount).ToList();
        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quizId,
            Status = "InProgress",
            StartedAt = DateTime.UtcNow,
            RemainingSeconds = quiz.TimeLimitMinutes * 60
        };
        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        foreach(var q in questions)
            _db.QuizAnswers.Add(new QuizAnswer { AttemptId = attempt.Id, QuestionId = q.Id });
        await _db.SaveChangesAsync();

        var qResponses = questions.Select(
            q => new QuizQuestionResponse(q.Id, q.QuestionText, q.OptionA, q.OptionB, q.OptionC, q.OptionD))
            .ToList();
        return new StartQuizResponse(
            attempt.Id,
            quiz.TimeLimitMinutes,
            attempt.StartedAt,
            qResponses,
            null,
            attempt.RemainingSeconds,
            remainingAttempts);
    }

    public async Task SaveAnswerDraftAsync(int attemptId, int userId, int questionId, string? selected)
    {
        var ans = await _db.QuizAnswers.FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.QuestionId == questionId);
        if(ans != null)
        {
            ans.SelectedAnswer = selected;
            ans.AnsweredAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task UpdateRemainingTimeAsync(int attemptId, int userId, int remainingSeconds)
    {
        var attempt = await _db.QuizAttempts.FindAsync(attemptId);
        if(attempt != null)
        {
            attempt.RemainingSeconds = remainingSeconds;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<QuizResultResponse>> GetUserQuizResultsAsync(int userId, int quizId)
    {
        return await _db.QuizAttempts
            .Where(a => a.UserId == userId && a.QuizId == quizId && a.Status == "Submitted")
            .Select(
                a => new QuizResultResponse(
                    a.Id,
                    a.Score ?? 0,
                    a.Answers.Count(ans => ans.IsCorrect == true),
                    a.Answers.Count,
                    a.IsPassed,
                    new List<QuizAnswerDetailResponse>(),
                    0,
                    0))
            .ToListAsync();
    }
}