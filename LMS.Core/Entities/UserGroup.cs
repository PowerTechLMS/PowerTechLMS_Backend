namespace LMS.Core.Entities;

// [14] Nhóm người dùng
public class UserGroup : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CreatedById { get; set; }

    public User? CreatedBy { get; set; }
    public ICollection<UserGroupMember> Members { get; set; } = new List<UserGroupMember>();
    public ICollection<Enrollment> GroupEnrollments { get; set; } = new List<Enrollment>();
}

public class UserGroupMember : BaseEntity
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int UserId { get; set; }
    public int? AddedById { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public UserGroup Group { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? AddedBy { get; set; }
}
