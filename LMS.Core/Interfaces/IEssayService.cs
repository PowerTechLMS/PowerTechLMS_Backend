using LMS.Core.DTOs;
using LMS.Core.Entities;

namespace LMS.Core.Interfaces;

public interface IEssayService
{
    Task<EssayAttempt> StartAttemptAsync(int userId, int lessonId);

    Task<EssayResultResponse> SubmitAttemptAsync(int userId, int attemptId, SubmitEssayRequest request);

    Task<List<EssayAttemptSummary>> GetAttemptsByLessonAsync(int userId, int lessonId, bool isAdmin = false);

    Task<EssayResultResponse> GetAttemptDetailAsync(int userId, int attemptId, bool isAdmin = false);

    Task<EssayAttempt?> GetActiveAttemptAsync(int userId, int lessonId);

    Task SaveDraftAsync(int userId, int attemptId, SubmitEssayRequest request);

    Task<List<AdminEssayAttemptResponse>> GetAllAttemptsAsync();

    Task UpdateAttemptAsync(int attemptId, AdminUpdateEssayAttemptRequest request);

    Task<List<EssayQuestionDto>> GenerateQuestionsFromLessonsAsync(List<int> lessonIds);

    Task<int> GetAttemptNumberAsync(int userId, int lessonId, DateTime startedAt);

    Task<List<EssayQuestionDto>> GetQuestionsByLessonAsync(int lessonId);

    Task IncrementViolationCountAsync(int attemptId);

    Task DeleteEssayQuestionAsync(int questionId);
}
