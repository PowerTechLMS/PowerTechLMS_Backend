using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class Lesson : BaseEntity
{
    public int Id { get; set; }

    public int ModuleId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Type { get; set; } = "Video";

    public string? Content { get; set; }

    public string? VideoUrl { get; set; }

    public string? VideoStorageKey { get; set; }

    public string? VideoStorageUrl { get; set; }

    public string? VideoProvider { get; set; }

    public string? VideoThumbnailUrl { get; set; }

    public int VideoDurationSeconds { get; set; } = 0;

    public int ReadingDurationSeconds { get; set; } = 0;

    public string VideoStatus { get; set; } = "Ready";

    public bool IsAiProcessed { get; set; } = false;

    public string? AiSummary { get; set; }

    public string? Transcript { get; set; }

    public string? SubtitlesPath { get; set; }

    public int SortOrder { get; set; }

    public bool IsFreePreview { get; set; } = false;

    public int? QuizId { get; set; }

    [ForeignKey("QuizId")]
    public virtual Quiz Quiz { get; set; } = null!;

    public Module Module { get; set; } = null!;

    public ICollection<LessonAttachment> Attachments { get; set; } = new List<LessonAttachment>();

    public ICollection<LessonProgress> Progresses { get; set; } = new List<LessonProgress>();

    public ICollection<QAThread> QAThreads { get; set; } = new List<QAThread>();

    public virtual RolePlayConfig? RolePlayConfig { get; set; }

    public virtual EssayConfig? EssayConfig { get; set; }

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}