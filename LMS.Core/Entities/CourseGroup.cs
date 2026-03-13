namespace LMS.Core.Entities;

// [14] Nhóm khóa học
public class CourseGroup : BaseEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CreatedById { get; set; }

    public User? CreatedBy { get; set; }
    public ICollection<CourseGroupCourse> Courses { get; set; } = new List<CourseGroupCourse>();
}

public class CourseGroupCourse : BaseEntity
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int CourseId { get; set; }
    public int SortOrder { get; set; } = 0;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public CourseGroup Group { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
