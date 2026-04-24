using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

[Table("AdminAiSessions")]
public class AdminAiSession : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(100)]
    public string? ThreadId { get; set; }

    public int CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public virtual User CreatedBy { get; set; } = default!;

    public virtual ICollection<AdminAiMessage> Messages { get; set; } = new List<AdminAiMessage>();

    public virtual ICollection<AiTask> Tasks { get; set; } = new List<AiTask>();

    public string? LastProgressJson { get; set; }
}
