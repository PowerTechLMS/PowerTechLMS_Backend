namespace LMS.Core.Entities;

public class Note : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? VideoTimestampSeconds { get; set; }

    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
