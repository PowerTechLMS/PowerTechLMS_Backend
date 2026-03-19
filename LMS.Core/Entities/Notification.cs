using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class Notification : BaseEntity
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? Link { get; set; } // URL to redirect when clicked
    
    public bool IsRead { get; set; } = false;
    
    public string? Type { get; set; } // "EnrollmentRequest", "EnrollmentStatus", "NewCourse", "NewCourseGroup", "Reminder"
    
    // Navigation
    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}
