
namespace LMS.Core.Entities;

public class LessonProgress : BaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LessonId { get; set; }

    public bool IsCompleted { get; set; } = false;

    public int PositionSeconds { get; set; } = 0;

    public int WatchedPercent { get; set; } = 0;

    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = null!;

    public Lesson Lesson { get; set; } = null!;
}
