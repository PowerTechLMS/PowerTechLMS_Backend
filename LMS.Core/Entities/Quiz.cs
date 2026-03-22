
namespace LMS.Core.Entities;

public class Quiz : BaseEntity
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int? TimeLimitMinutes { get; set; }

    public int PassScore { get; set; } = 8;

    public int QuestionCount { get; set; } = 10;

    public bool ShuffleQuestions { get; set; } = true;

    public bool ShuffleAnswers { get; set; } = true;

    public int? MaxAttemptsPerWindow { get; set; }

    public int? AttemptWindowHours { get; set; }

    public int? AvailableFromDays { get; set; }

    public DateTime? QuizStartDate { get; set; }

    public DateTime? QuizEndDate { get; set; }

    public int? RetakeWaitTimeMinutes { get; set; }

    public int? MaxRetakesPerDay { get; set; }

    public ICollection<QuestionBank> Questions { get; set; } = new List<QuestionBank>();

    public Course Course { get; set; } = null!;

    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
