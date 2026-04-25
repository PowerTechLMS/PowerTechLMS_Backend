using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class LessonInfographic : BaseEntity
{
    public int Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public int CreatedById { get; set; }

    [ForeignKey("CreatedById")]
    public virtual User CreatedBy { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
