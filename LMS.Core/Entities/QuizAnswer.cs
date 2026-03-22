
namespace LMS.Core.Entities;

public class QuizAnswer : BaseEntity
{
    public int Id { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public string? SelectedAnswer { get; set; }

    public bool? IsCorrect { get; set; }

    public DateTime? AnsweredAt { get; set; }

    public QuizAttempt Attempt { get; set; } = null!;

    public QuestionBank Question { get; set; } = null!;
}
