using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class EssayConfig : BaseEntity
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public string? SupportLessonIds { get; set; }

    public int? TimeLimitMinutes { get; set; }

    public int? MaxAttemptsPerWindow { get; set; }

    public int? AttemptWindowHours { get; set; }

    public int PassScore { get; set; } = 50;

    [ForeignKey("LessonId")]
    public virtual Lesson Lesson { get; set; } = null!;

    public virtual ICollection<EssayQuestion> Questions { get; set; } = new List<EssayQuestion>();
}
