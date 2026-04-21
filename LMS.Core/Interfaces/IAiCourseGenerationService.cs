
namespace LMS.Core.Interfaces;

public interface IAiCourseGenerationService
{
    Task<string> StartCourseGenerationAsync(int userId, string topic, string targetAudience, string additionalInfo);

    Task<CourseGenerationProgress> GetProgressAsync(string jobId);

    Task GenerateLessonVideoFrameAsync(int lessonId);
}

public class CourseGenerationProgress
{
    public string JobId { get; set; } = string.Empty;

    public int Progress { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? ResultJson { get; set; }

    public List<AiSubTask>? SubTasks { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsFailed { get; set; }

    public string? ErrorMessage { get; set; }
}

public class AiSubTask
{
    public string Id { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = "waiting";
}
