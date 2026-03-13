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
    // [2] Thời hạn đăng ký
    public DateTime? EnrollStartDate { get; set; }
    public DateTime? EnrollEndDate { get; set; }
    // [3] Thời hạn hoàn thành
    public int? CompletionDeadlineDays { get; set; } // N ngày kể từ enroll
    public DateTime? CompletionEndDate { get; set; }  // Ngày tuyệt đối

    // Navigation
    public User CreatedBy { get; set; } = null!;
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    public CertificateTemplate? CertificateTemplate { get; set; }
    
    // [4] Workflow duyệt ghi danh
    public bool RequiresApproval { get; set; } = true;

    // 1. Cột lưu ID của danh mục (Khóa ngoại)
    public int? CategoryId { get; set; }

    // 2. Thuộc tính điều hướng (Navigation Property) để nối bảng - THÊM DÒNG NÀY VÀO
    public Category? Category { get; set; }
}
