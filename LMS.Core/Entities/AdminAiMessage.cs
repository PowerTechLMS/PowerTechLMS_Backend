using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

[Table("AdminAiMessages")]
public class AdminAiMessage : BaseEntity
{
    [Key]
    public int Id { get; set; }

    public int SessionId { get; set; }

    [ForeignKey("SessionId")]
    public virtual AdminAiSession Session { get; set; } = default!;

    [Required]
    [MaxLength(20)]
    public required string Role { get; set; }

    [Required]
    public required string Content { get; set; }

    public string? ToolCallsJson { get; set; }

    public string? PlanJson { get; set; }
}
