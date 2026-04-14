using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class RolePlayConfig : BaseEntity
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public string? SupportLessonIds { get; set; }

    public string ScoringCriteria { get; set; } = string.Empty;

    public string? AdditionalRequirements { get; set; }

    public string? Scenario { get; set; }

    public int PassScore { get; set; } = 50;

    [ForeignKey("LessonId")]
    public virtual Lesson Lesson { get; set; } = null!;
}
