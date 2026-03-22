
namespace LMS.Core.Entities;

public class QAThread : BaseEntity
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    public Lesson Lesson { get; set; } = null!;

    public User User { get; set; } = null!;

    public QAThread? Parent { get; set; }

    public ICollection<QAThread> Replies { get; set; } = new List<QAThread>();
}
