
namespace LMS.Core.Entities;

public class DocumentVersion : BaseEntity
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public int VersionNumber { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string? StorageKey { get; set; }

    public long FileSize { get; set; } = 0;

    public string? FileType { get; set; }

    public string? ChangeNote { get; set; }

    public int UploadedById { get; set; }

    public Document Document { get; set; } = null!;

    public User UploadedBy { get; set; } = null!;
}

public class DocumentPermission : BaseEntity
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public int? UserId { get; set; }

    public int? RoleId { get; set; }

    public int? UserGroupId { get; set; }

    public bool CanViewCurrent { get; set; } = true;

    public bool CanViewHistory { get; set; } = false;

    public int? GrantedById { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public Document Document { get; set; } = null!;

    public User? User { get; set; }

    public Role? Role { get; set; }

    public UserGroup? UserGroup { get; set; }

    public User? GrantedBy { get; set; }
}
