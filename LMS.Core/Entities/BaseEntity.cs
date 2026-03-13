namespace LMS.Core.Entities;

/// <summary>Base class cho tất cả entities – soft delete + audit columns</summary>
public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted    { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
}
