namespace LMS.Core.Entities;

// [9] Phiên bản tài liệu
public class DocumentVersion : BaseEntity
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int VersionNumber { get; set; }      // Tăng dần: 1, 2, 3...
    public string FileName { get; set; } = string.Empty;
    public string? StorageKey { get; set; }     // Key trên cloud storage
    public long FileSize { get; set; } = 0;
    public string? FileType { get; set; }
    public string? ChangeNote { get; set; }     // Ghi chú thay đổi
    public int UploadedById { get; set; }

    public Document Document { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
}

// [12] Quyền xem tài liệu
public class DocumentPermission : BaseEntity
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    // Gán theo User HOẶC Role HOẶC UserGroup (chỉ 1 trong 3)
    public int? UserId { get; set; }
    public int? RoleId { get; set; }
    public int? UserGroupId { get; set; }
    // Loại quyền
    public bool CanViewCurrent { get; set; } = true;  // Xem phiên bản hiện tại
    public bool CanViewHistory { get; set; } = false; // Xem phiên bản cũ
    // Metadata
    public int? GrantedById { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public Document Document { get; set; } = null!;
    public User? User { get; set; }
    public Role? Role { get; set; }
    public UserGroup? UserGroup { get; set; }
    public User? GrantedBy { get; set; }
}
