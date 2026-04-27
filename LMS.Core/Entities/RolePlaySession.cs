using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class RolePlaySession : BaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int LessonId { get; set; }

    public string Status { get; set; } = "InProgress";

    public int? Score { get; set; }

    public string? Feedback { get; set; }

    [ForeignKey("UserId")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("LessonId")]
    public virtual Lesson Lesson { get; set; } = null!;

    public int ViolationCount { get; set; } = 0;

    public virtual ICollection<RolePlayMessage> Messages { get; set; } = new List<RolePlayMessage>();
}
