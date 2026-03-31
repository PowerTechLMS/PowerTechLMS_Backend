
namespace LMS.Core.Entities;

public class User : BaseEntity
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Employee";

    public string? Position { get; set; }

    public string? Avatar { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Bio { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? DeactivatedAt { get; set; }

    public int? DeactivatedById { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

    public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();

    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    public ICollection<QAThread> QAThreads { get; set; } = new List<QAThread>();

    public ICollection<Note> Notes { get; set; } = new List<Note>();

    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
