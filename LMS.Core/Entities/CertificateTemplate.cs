namespace LMS.Core.Entities;

// [15] Mẫu chứng chỉ theo từng khóa học
public class CertificateTemplate : BaseEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }           // 1 khóa học = 1 template
    public string TemplateName { get; set; } = "Mẫu mặc định";
    public bool UseBuiltInTemplate { get; set; } = true; // true = dùng template hệ thống

    // Custom HTML template (nếu UseBuiltInTemplate = false)
    public string? HtmlTemplate { get; set; }

    // Built-in template fields
    public string? BackgroundImageUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? SignatureImageUrl { get; set; }
    public string? SignatureName { get; set; }  // Tên người ký
    public string? SignatureTitle { get; set; } // Chức vụ
    public string TitleText { get; set; } = "CHỨNG CHỈ HOÀN THÀNH";
    public string? BodyText { get; set; }       // Hỗ trợ {{FullName}}, {{CourseName}}, {{Date}}
    public string? FooterText { get; set; }
    public string PrimaryColor { get; set; } = "#1a56db";

    public Course Course { get; set; } = null!;
}
