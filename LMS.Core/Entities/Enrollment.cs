
namespace LMS.Core.Entities;

public class Enrollment : BaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime? Deadline { get; set; }

    public int? AssignedById { get; set; }

    public int? GroupEnrollId { get; set; }

    public bool IsMandatory { get; set; } = false;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAt { get; set; }

    public User User { get; set; } = null!;

    public Course Course { get; set; } = null!;

    public User? AssignedBy { get; set; }

    public UserGroup? GroupEnroll { get; set; }
}
