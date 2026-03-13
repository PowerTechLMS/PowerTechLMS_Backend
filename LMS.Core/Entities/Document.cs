namespace LMS.Core.Entities;

public class Document : BaseEntity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public int UploadedById { get; set; }
    // [9] Phiên bản hiện tại
    public int? CurrentVersionId { get; set; }
    // [8] Thời hạn xem tài liệu
    public DateTime? AccessStartDate { get; set; }
    public DateTime? AccessEndDate { get; set; }

    public User UploadedBy { get; set; } = null!;
    public DocumentVersion? CurrentVersion { get; set; }
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
}

