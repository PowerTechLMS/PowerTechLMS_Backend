
namespace LMS.Core.Entities;

public class QuizAttempt : BaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int QuizId { get; set; }

    public string Status { get; set; } = "InProgress";

    public decimal? Score { get; set; }

    public bool IsPassed { get; set; } = false;

    public int? RemainingSeconds { get; set; }

    public DateTime? LastActiveAt { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public User User { get; set; } = null!;

    public Quiz Quiz { get; set; } = null!;

    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
}
