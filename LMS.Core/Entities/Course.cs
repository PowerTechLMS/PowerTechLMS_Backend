
namespace LMS.Core.Entities;

public class Course : BaseEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? CoverImageUrl { get; set; }

    public int CreatedById { get; set; }

    public bool IsPublished { get; set; } = false;

    public int PassScore { get; set; } = 8;

    public DateTime? EnrollStartDate { get; set; }

    public DateTime? EnrollEndDate { get; set; }

    public int? CompletionDeadlineDays { get; set; }

    public DateTime? CompletionEndDate { get; set; }

    public User CreatedBy { get; set; } = null!;

    public ICollection<Module> Modules { get; set; } = new List<Module>();

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public CertificateTemplate? CertificateTemplate { get; set; }

    public bool RequiresApproval { get; set; } = true;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public int QuizRetakeWaitTimeMinutes { get; set; } = 5;

    public int QuizMaxRetakesPerDay { get; set; } = 3;

    public int Level { get; set; } = 3;

    public int? UserGroupId { get; set; }

    public UserGroup? UserGroup { get; set; }
}
