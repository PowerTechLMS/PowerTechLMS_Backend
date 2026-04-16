using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class EssayAnswer : BaseEntity
{
    public int Id { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int? AiScore { get; set; }

    public string? AiFeedback { get; set; }

    [ForeignKey("AttemptId")]
    public virtual EssayAttempt Attempt { get; set; } = null!;

    [ForeignKey("QuestionId")]
    public virtual EssayQuestion Question { get; set; } = null!;
}
