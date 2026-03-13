using System.ComponentModel.DataAnnotations.Schema; // BẮT BUỘC THÊM DÒNG NÀY

namespace LMS.Core.Entities;

public class Lesson : BaseEntity
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = "Video"; // Video | Text
    public string? Content { get; set; }
    // YouTube / embed URL
    public string? VideoUrl { get; set; }
    // [10] Hosted video storage
    public string? VideoStorageKey { get; set; }
    public string? VideoStorageUrl { get; set; }
    public string? VideoProvider { get; set; }  // YouTube | S3 | Azure | Vimeo
    public string? VideoThumbnailUrl { get; set; }
    public int VideoDurationSeconds { get; set; } = 0;
    public int SortOrder { get; set; }
    public bool IsFreePreview { get; set; } = false;

    public int? QuizId { get; set; }

    [ForeignKey("QuizId")] // <--- THÊM DÒNG NÀY ĐỂ FIX LỖI QuizId1
    public virtual Quiz Quiz { get; set; }

    public Module Module { get; set; } = null!;
    public ICollection<LessonAttachment> Attachments { get; set; } = new List<LessonAttachment>();
    public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();
    public ICollection<QAThread> QAThreads { get; set; } = new List<QAThread>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}