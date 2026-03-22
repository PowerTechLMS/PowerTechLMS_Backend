
namespace LMS.Core.Entities;

public class Permission : BaseEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class Role : BaseEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystem { get; set; } = false;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class RolePermission : BaseEntity
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    public int? GrantedById { get; set; }

    public Role Role { get; set; } = null!;

    public Permission Permission { get; set; } = null!;
}

public class UserRole : BaseEntity
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public int? AssignedById { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public User User { get; set; } = null!;

    public Role Role { get; set; } = null!;
}
