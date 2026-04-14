using LMS.Core.Entities;
using LMS.Core.DTOs;

namespace LMS.Core.Interfaces;

public interface IRolePlayService
{
    Task<RolePlaySession> StartSessionAsync(int userId, int lessonId);
    Task<RolePlayMessage> SendMessageAsync(int userId, int sessionId, string content);
    IAsyncEnumerable<string> SendMessageStreamingAsync(int userId, int sessionId, string content);
    Task FinishSessionAsync(int userId, int sessionId);
    Task<RolePlaySession?> GetSessionAsync(int userId, int lessonId);
    Task<List<RolePlaySession>> GetSessionsByLessonAsync(int userId, int lessonId);
    Task<RolePlaySession?> GetSessionByIdAsync(int userId, int sessionId);
    Task<List<RolePlaySession>> GetUserSessionsAsync(int userId);
    Task<List<RolePlaySessionResponse>> GetAllSessionsAsync();
    Task UpdateSessionStatusAsync(int sessionId, string status, int? score, string? feedback);
    Task<RolePlaySuggestionResponse> GenerateScenarioFromLessonsAsync(List<int> lessonIds);
}

public record RolePlaySuggestionResponse(string Scenario, string ScoringCriteria, string AdditionalRequirements);
