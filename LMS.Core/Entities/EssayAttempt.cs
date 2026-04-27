using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class EssayAttempt : BaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LessonId { get; set; }

    public string Status { get; set; } = "InProgress";

    public int? TotalScore { get; set; }

    public bool IsPassed { get; set; } = false;

    public string? AiFeedback { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("LessonId")]
    public virtual Lesson Lesson { get; set; } = null!;

    public int ViolationCount { get; set; } = 0;

    public virtual ICollection<EssayAnswer> Answers { get; set; } = new List<EssayAnswer>();
}
