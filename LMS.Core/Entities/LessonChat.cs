using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class LessonChat : BaseEntity
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public int UserId { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    public string AiResponse { get; set; } = string.Empty;

    public double? VideoTimestamp { get; set; }

    [ForeignKey("LessonId")]
    public virtual Lesson Lesson { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;
}
