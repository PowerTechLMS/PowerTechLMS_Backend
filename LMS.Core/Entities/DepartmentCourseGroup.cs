using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class DepartmentCourseGroup
{
    public int DepartmentId { get; set; }

    public int CourseGroupId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public int? AssignedById { get; set; }

    [ForeignKey("DepartmentId")]
    public UserGroup Department { get; set; } = null!;

    [ForeignKey("CourseGroupId")]
    public CourseGroup CourseGroup { get; set; } = null!;
}