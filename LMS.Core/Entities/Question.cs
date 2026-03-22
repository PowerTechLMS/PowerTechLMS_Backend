
namespace LMS.Core.Entities;

public class Question
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = "A";

    public double Points { get; set; } = 1.0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual Quiz? Quiz { get; set; }
}