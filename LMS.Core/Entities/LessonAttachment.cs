
namespace LMS.Core.Entities;

public class LessonAttachment : BaseEntity
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? StorageKey { get; set; }

    public long FileSize { get; set; }

    public Lesson Lesson { get; set; } = null!;
}
