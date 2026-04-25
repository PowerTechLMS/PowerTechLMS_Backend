
namespace LMS.Core.Entities;

public class Document : BaseEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Tags { get; set; }

    public int UploadedById { get; set; }

    public int? CurrentVersionId { get; set; }

    public DateTime? AccessStartDate { get; set; }

    public DateTime? AccessEndDate { get; set; }

    public bool IsAiProcessed { get; set; } = false;

    public string? AiSummary { get; set; }

    public bool IsOutdated { get; set; } = false;

    public DateTime? OutdatedAt { get; set; }

    public DateTime? LastOutdatedNotifiedAt { get; set; }

    public string? OutdatedReason { get; set; }

    public User UploadedBy { get; set; } = null!;

    public DocumentVersion? CurrentVersion { get; set; }

    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();

    public ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
}
