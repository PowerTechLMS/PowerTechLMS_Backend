using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class QuestionBank : BaseEntity
{
    public int Id { get; set; }

    public int QuizId { get; set; }

    public string QuestionText { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = "A";

    public decimal Points { get; set; } = 1.0m;

    public string? Explanation { get; set; }

    [ForeignKey("QuizId")]
    public Quiz Quiz { get; set; } = null!;

    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
}