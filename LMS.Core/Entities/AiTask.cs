using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

[Table("AiTasks")]
public class AiTask
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string JobId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Topic { get; set; } = string.Empty;

    public int Progress { get; set; }

    [MaxLength(2000)]
    public string Status { get; set; } = string.Empty;

    public string? ResultJson { get; set; }

    public string? SubTasksJson { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsFailed { get; set; }

    public string? ErrorMessage { get; set; }

    [Required]
    public int CreatedById { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CreatedById")]
    public User CreatedBy { get; set; } = null!;

    public int? SessionId { get; set; }

    [ForeignKey("SessionId")]
    public virtual AdminAiSession? Session { get; set; }
}
