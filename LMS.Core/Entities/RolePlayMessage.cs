using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class RolePlayMessage : BaseEntity
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [ForeignKey("SessionId")]
    public virtual RolePlaySession Session { get; set; } = null!;
}
