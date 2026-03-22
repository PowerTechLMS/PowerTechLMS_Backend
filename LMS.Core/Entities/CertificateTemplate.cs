
namespace LMS.Core.Entities;

public class CertificateTemplate : BaseEntity
{
    public int Id { get; set; }

    public int CourseId { get; set; }

    public string TemplateName { get; set; } = "Mẫu mặc định";

    public bool UseBuiltInTemplate { get; set; } = true;

    public string? HtmlTemplate { get; set; }

    public string? BackgroundImageUrl { get; set; }

    public string? LogoUrl { get; set; }

    public string? SignatureImageUrl { get; set; }

    public string? SignatureName { get; set; }

    public string? SignatureTitle { get; set; }

    public string TitleText { get; set; } = "CHỨNG CHỈ HOÀN THÀNH";

    public string? BodyText { get; set; }

    public string? FooterText { get; set; }

    public string PrimaryColor { get; set; } = "#1a56db";

    public Course Course { get; set; } = null!;
}
