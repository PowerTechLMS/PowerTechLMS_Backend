
namespace LMS.Core.Interfaces;

public record AiToolResponse(bool Success, string? Message, object? Data = null);

public record AiToolInfo(string Name, string Description, string[] Permissions);

public interface IAiToolService
{
    Task<AiToolResponse> ExecuteToolAsync(string toolName, string argumentsJson, int adminId);

    Task<object> AnalyzePerformanceAsync(string topic, int limit = 50);

    Task<object> GetUserAILearningHistoryAsync(int userId);

    Task<object> SearchEntitiesAsync(string type, string query, int limit = 50);

    Task<object> SearchVectorContentAsync(string query, int topK = 5);

    Task<AiToolResponse> CreateCourseAsync(string title, int? categoryId, int level, int adminId);

    Task<AiToolResponse> UpdateCourseContentAsync(int courseId, string? title, string? description);

    Task<AiToolResponse> GetCourseDetailsAsync(int courseId);

    Task<AiToolResponse> GenerateLessonContentAsync(int moduleId, string topic, string type);

    Task<AiToolResponse> MassEnrollAsync(List<int> userIds, int courseId, int adminId);

    Task<List<AiToolInfo>> GetAvailableToolsAsync(int adminId, string? query = null);
}
