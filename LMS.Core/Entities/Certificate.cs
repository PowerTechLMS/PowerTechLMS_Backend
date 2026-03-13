namespace LMS.Core.Entities;

public class Certificate : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public string CertificateCode { get; set; } = Guid.NewGuid().ToString("N")[..12].ToUpper();
    public string? PdfUrl { get; set; }
    public int? AttemptId { get; set; } // Liên kết lần thi đạt
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Valid";
    public DateTime? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
    public QuizAttempt? Attempt { get; set; }
}
