using Hangfire;
using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;


namespace LMS.Infrastructure.Services;

public class ModuleService : IModuleService
{
    private readonly AppDbContext _db;
    private readonly VectorDbService _vectorDb;

    public ModuleService(AppDbContext db, VectorDbService vectorDb)
    {
        _db = db;
        _vectorDb = vectorDb;
    }


    public async Task<ModuleResponse> CreateModuleAsync(
        int courseId,
        CreateModuleRequest request,
        int userId,
        bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý chương cho khóa học này.");

        var module = new Module { CourseId = courseId, Title = request.Title, SortOrder = request.SortOrder };
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();
        return new ModuleResponse(module.Id, module.Title, module.SortOrder, new List<LessonResponse>());
    }

    public async Task<ModuleResponse> UpdateModuleAsync(
        int courseId,
        int moduleId,
        UpdateModuleRequest request,
        int userId,
        bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý chương cho khóa học này.");

        var module = await _db.Modules
                .Include(m => m.Lessons.OrderBy(l => l.SortOrder))
                .ThenInclude(l => l.Attachments)
                .Include(m => m.Lessons)
                .ThenInclude(l => l.Quiz)
                .ThenInclude(q => q.Questions)
                .Include(m => m.Lessons)
                .ThenInclude(l => l.RolePlayConfig)
                .Include(m => m.Lessons)
                .ThenInclude(l => l.EssayConfig)
                .ThenInclude(c => c!.Questions)
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId) ??
            throw new KeyNotFoundException("Không tìm thấy chương trong khóa học này.");

        module.Title = request.Title;
        module.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync();

        return new ModuleResponse(
            module.Id,
            module.Title,
            module.SortOrder,
            module.Lessons
                .Select(
                    l => new LessonResponse(
                            l.Id,
                            l.Title,
                            l.Type,
                            l.Content,
                            l.VideoStorageUrl ?? l.VideoUrl,
                            l.VideoDurationSeconds,
                            l.ReadingDurationSeconds,
                            l.VideoStatus,
                            l.SortOrder,
                            l.IsFreePreview,
                            l.Attachments.Select(a => new AttachmentResponse(a.Id, a.FileName, a.FileSize)).ToList(),
                            l.QuizId,
                            l.Quiz?.Questions.Count ?? 0,
                            l.AiSummary,
                            l.VideoDraftScript,
                            l.RolePlayConfig != null
                                ? new RolePlayConfigDto(
                                    JsonSerializer.Deserialize<List<int>>(l.RolePlayConfig.SupportLessonIds ?? "[]") ??
                                        new List<int>(),
                                    l.RolePlayConfig.ScoringCriteria,
                                    l.RolePlayConfig.AdditionalRequirements,
                                    l.RolePlayConfig.Scenario)
                                : null,
                            l.EssayConfig != null
                                ? new EssayConfigDto(
                                    JsonSerializer.Deserialize<List<int>>(l.EssayConfig.SupportLessonIds ?? "[]") ??
                                        new List<int>(),
                                    l.EssayConfig.TimeLimitMinutes,
                                    l.EssayConfig.MaxAttemptsPerWindow,
                                    l.EssayConfig.AttemptWindowHours,
                                    l.EssayConfig.PassScore,
                                    l.EssayConfig.Questions
                                        .Select(
                                            q => new EssayQuestionDto(
                                                                    q.Id,
                                                                    q.Content,
                                                                    q.SortOrder,
                                                                    q.Weight,
                                                                    q.ScoringCriteria))
                                        .ToList())
                                : null))
                .ToList());
    }

    public async Task DeleteModuleAsync(int courseId, int moduleId, int userId, bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý chương cho khóa học này.");

        var module = await _db.Modules
                .Include(m => m.Lessons)
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId) ??
            throw new KeyNotFoundException("Không tìm thấy chương trong khóa học này.");

        foreach(var lesson in module.Lessons)
        {
            await _vectorDb.DeleteVectorsByFilterAsync("LessonId", lesson.Id);
        }

        module.IsDeleted = true;
        module.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(int courseId, List<SortOrderItem> items, int userId, bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý chương cho khóa học này.");

        foreach(var item in items)
        {
            var module = await _db.Modules.FirstOrDefaultAsync(m => m.Id == item.Id && m.CourseId == courseId);
            if(module != null)
                module.SortOrder = item.SortOrder;
        }
        await _db.SaveChangesAsync();
    }
}

public class LessonService : ILessonService
{
    private readonly AppDbContext _db;
    private readonly VectorDbService _vectorDb;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _config;

    public LessonService(
        AppDbContext db,
        VectorDbService vectorDb,
        INotificationService notificationService,
        IConfiguration config)
    {
        _db = db;
        _vectorDb = vectorDb;
        _notificationService = notificationService;
        _config = config;
    }


    public async Task<(Stream stream, string fileName, string contentType)> GetAttachmentFileAsync(int attachmentId)
    {
        var attachment = await _db.LessonAttachments.FindAsync(attachmentId) ??
            throw new KeyNotFoundException("Không tìm thấy tài liệu đính kèm.");

        var storageKey = attachment.StorageKey ?? string.Empty;
        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var filePath = Path.Combine(wwwroot, "uploads", storageKey.TrimStart('/'));
        if(!File.Exists(filePath))
            throw new FileNotFoundException("Tệp vật lý không tồn tại trên máy chủ.");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };

        return (stream, attachment.FileName, contentType);
    }

    public async Task<LessonResponse> CreateLessonAsync(
        int moduleId,
        CreateLessonRequest request,
        int userId,
        bool isAdmin = false)
    {
        var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy chương.");

        if(!isAdmin && module.Course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý bài học cho khóa học này.");

        var lesson = new Lesson
        {
            ModuleId = moduleId,
            Title = request.Title,
            Type = request.Type,
            Content = request.Content,
            VideoUrl = request.VideoUrl,
            SortOrder = request.SortOrder,
            IsFreePreview = request.IsFreePreview,
            VideoStatus = request.VideoStatus,
            VideoDurationSeconds = request.VideoDurationSeconds,
            ReadingDurationSeconds = request.ReadingDurationSeconds,
            VideoDraftScript = request.VideoDraftScript
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();

        if(request.Type == "RolePlay" && request.RolePlayConfig != null)
        {
            var rpConfig = new RolePlayConfig
            {
                LessonId = lesson.Id,
                SupportLessonIds = JsonSerializer.Serialize(request.RolePlayConfig.SupportLessonIds),
                ScoringCriteria = request.RolePlayConfig.ScoringCriteria,
                AdditionalRequirements = request.RolePlayConfig.AdditionalRequirements,
                Scenario = request.RolePlayConfig.Scenario,
                PassScore = request.RolePlayConfig.PassScore
            };
            _db.RolePlayConfigs.Add(rpConfig);
            await _db.SaveChangesAsync();
        }

        if(request.Type == "Essay" && request.EssayConfig != null)
        {
            var essayConfig = new EssayConfig
            {
                LessonId = lesson.Id,
                SupportLessonIds = JsonSerializer.Serialize(request.EssayConfig.SupportLessonIds),
                TimeLimitMinutes = request.EssayConfig.TimeLimitMinutes,
                MaxAttemptsPerWindow = request.EssayConfig.MaxAttemptsPerWindow,
                AttemptWindowHours = request.EssayConfig.AttemptWindowHours,
                PassScore = request.EssayConfig.PassScore
            };
            _db.EssayConfigs.Add(essayConfig);
            await _db.SaveChangesAsync();

            foreach(var q in request.EssayConfig.Questions)
            {
                var question = new EssayQuestion
                {
                    EssayConfigId = essayConfig.Id,
                    Content = q.Content,
                    SortOrder = q.SortOrder,
                    Weight = q.Weight
                };
                _db.EssayQuestions.Add(question);
            }
            await _db.SaveChangesAsync();
        }

        if(lesson.Type == "Text" && !string.IsNullOrWhiteSpace(lesson.Content))
        {
            BackgroundJob.Enqueue<IAiProcessingService>(x => x.ProcessLessonTextAsync(lesson.Id));
        }

        await NotifyEnrolledUsersAsync(lesson.Id, "NewLesson");

        return new LessonResponse(
            lesson.Id,
            lesson.Title,
            lesson.Type,
            lesson.Content,
            lesson.VideoStorageUrl ?? lesson.VideoUrl,
            lesson.VideoDurationSeconds,
            lesson.ReadingDurationSeconds,
            lesson.VideoStatus,
            lesson.SortOrder,
            lesson.IsFreePreview,
            new List<AttachmentResponse>(),
            lesson.QuizId,
            0,
            lesson.AiSummary,
            lesson.VideoDraftScript,
            request.RolePlayConfig,
            request.EssayConfig);
    }

    public async Task<int> CreateLessonQuizAsync(int lessonId, CreateQuizRequest request)
    {
        var lesson = await _db.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);

        if(lesson == null)
            throw new KeyNotFoundException("Không tìm thấy bài học.");

        var newQuiz = new Quiz
        {
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Bài tập củng cố" : request.Title,
            TimeLimitMinutes = request.TimeLimitMinutes ?? 0,
            PassScore = request.PassScore,
            QuestionCount = request.QuestionCount,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleAnswers = request.ShuffleAnswers,
            CourseId = lesson.Module.CourseId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Quizzes.Add(newQuiz);
        await _db.SaveChangesAsync();

        lesson.QuizId = newQuiz.Id;
        await _db.SaveChangesAsync();

        return newQuiz.Id;
    }

    public async Task<LessonResponse> UpdateLessonAsync(
        int moduleId,
        int lessonId,
        UpdateLessonRequest request,
        int userId,
        bool isAdmin = false)
    {
        var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy chương.");

        if(!isAdmin && module.Course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý bài học cho khóa học này.");

        var lesson = await _db.Lessons
                .Include(l => l.Attachments)
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.ModuleId == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy bài học trong chương này.");

        bool typeChanged = lesson.Type != request.Type;
        lesson.Title = request.Title;
        lesson.Type = request.Type;
        lesson.Content = request.Content;
        lesson.VideoUrl = request.VideoUrl;
        lesson.SortOrder = request.SortOrder;
        lesson.IsFreePreview = request.IsFreePreview;
        lesson.VideoDurationSeconds = request.VideoDurationSeconds;
        lesson.ReadingDurationSeconds = request.ReadingDurationSeconds;
        lesson.VideoStatus = request.VideoStatus;
        lesson.VideoDraftScript = request.VideoDraftScript;

        if(typeChanged)
        {
            var progresses = await _db.LessonProgresses.Where(lp => lp.LessonId == lessonId).ToListAsync();
            foreach(var p in progresses)
            {
                p.IsCompleted = false;
                p.PositionSeconds = 0;
                p.WatchedPercent = 0;
                p.CompletedAt = null;
            }

            lesson.AiSummary = null;
            lesson.IsAiProcessed = false;
            lesson.Transcript = null;
        }

        if(lesson.Type == "RolePlay" && request.RolePlayConfig != null)
        {
            var rpConfig = await _db.RolePlayConfigs.FirstOrDefaultAsync(c => c.LessonId == lessonId);
            if(rpConfig == null)
            {
                rpConfig = new RolePlayConfig { LessonId = lessonId };
                _db.RolePlayConfigs.Add(rpConfig);
            }
            rpConfig.SupportLessonIds = JsonSerializer.Serialize(request.RolePlayConfig.SupportLessonIds);
            rpConfig.AdditionalRequirements = request.RolePlayConfig.AdditionalRequirements;
            rpConfig.Scenario = request.RolePlayConfig.Scenario;
            rpConfig.PassScore = request.RolePlayConfig.PassScore;
        }

        if(lesson.Type == "Essay" && request.EssayConfig != null)
        {
            var essayConfig = await _db.EssayConfigs
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.LessonId == lessonId);
            if(essayConfig == null)
            {
                essayConfig = new EssayConfig { LessonId = lessonId };
                _db.EssayConfigs.Add(essayConfig);
            }
            essayConfig.SupportLessonIds = JsonSerializer.Serialize(request.EssayConfig.SupportLessonIds);
            essayConfig.TimeLimitMinutes = request.EssayConfig.TimeLimitMinutes;
            essayConfig.MaxAttemptsPerWindow = request.EssayConfig.MaxAttemptsPerWindow;
            essayConfig.AttemptWindowHours = request.EssayConfig.AttemptWindowHours;
            essayConfig.PassScore = request.EssayConfig.PassScore;

            var existingQuestions = essayConfig.Questions.ToList();
            var incomingQuestionIds = request.EssayConfig.Questions
                .Where(q => q.Id.HasValue && q.Id.Value > 0)
                .Select(q => q.Id!.Value)
                .ToList();

            var questionsToRemove = existingQuestions.Where(q => !incomingQuestionIds.Contains(q.Id)).ToList();
            foreach(var qToRemove in questionsToRemove)
            {
                var hasAnswers = await _db.EssayAnswers.AnyAsync(a => a.QuestionId == qToRemove.Id);
                if(!hasAnswers)
                {
                    _db.EssayQuestions.Remove(qToRemove);
                }
            }

            foreach(var q in request.EssayConfig.Questions)
            {
                if(q.Id.HasValue && q.Id.Value > 0)
                {
                    var existing = existingQuestions.FirstOrDefault(ex => ex.Id == q.Id.Value);
                    if(existing != null)
                    {
                        existing.Content = q.Content;
                        existing.SortOrder = q.SortOrder;
                        existing.Weight = q.Weight;
                        existing.ScoringCriteria = q.ScoringCriteria;
                    } else
                    {
                        _db.EssayQuestions
                            .Add(
                                new EssayQuestion
                                {
                                    EssayConfigId = essayConfig.Id,
                                    Content = q.Content,
                                    SortOrder = q.SortOrder,
                                    Weight = q.Weight,
                                    ScoringCriteria = q.ScoringCriteria
                                });
                    }
                } else
                {
                    _db.EssayQuestions
                        .Add(
                            new EssayQuestion
                            {
                                EssayConfigId = essayConfig.Id,
                                Content = q.Content,
                                SortOrder = q.SortOrder,
                                Weight = q.Weight,
                                ScoringCriteria = q.ScoringCriteria
                            });
                }
            }
        }

        await _db.SaveChangesAsync();

        if(lesson.Type == "Text" && !string.IsNullOrWhiteSpace(lesson.Content))
        {
            BackgroundJob.Enqueue<IAiProcessingService>(x => x.ProcessLessonTextAsync(lesson.Id));
        }

        await NotifyEnrolledUsersAsync(lesson.Id, "LessonUpdated");

        return new LessonResponse(
            lesson.Id,
            lesson.Title,
            lesson.Type,
            lesson.Content,
            lesson.VideoStorageUrl ?? lesson.VideoUrl,
            lesson.VideoDurationSeconds,
            lesson.ReadingDurationSeconds,
            lesson.VideoStatus,
            lesson.SortOrder,
            lesson.IsFreePreview,
            lesson.Attachments.Select(a => new AttachmentResponse(a.Id, a.FileName, a.FileSize)).ToList(),
            lesson.QuizId,
            lesson.Quiz?.Questions.Count ?? 0,
            lesson.AiSummary,
            lesson.VideoDraftScript,
            request.RolePlayConfig,
            request.EssayConfig);
    }

    public async Task DeleteLessonAsync(int moduleId, int lessonId, int userId, bool isAdmin = false)
    {
        var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy chương.");

        if(!isAdmin && module.Course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý bài học cho khóa học này.");

        var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId && l.ModuleId == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy bài học trong chương này.");

        await _vectorDb.DeleteVectorsByFilterAsync("LessonId", lessonId);

        // Find infographics that include this lesson
        var relatedInfographics = await _db.LessonInfographics
            .Include(li => li.Lessons)
            .Where(li => li.Lessons.Any(l => l.Id == lessonId))
            .ToListAsync();

        foreach (var info in relatedInfographics)
        {
            // Delete physical file
            try
            {
                var storageRoot = _config["Storage:RootPath"];
                var rootPath = string.IsNullOrEmpty(storageRoot)
                    ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
                    : storageRoot;

                var filePath = Path.Combine(rootPath, info.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch { }
            _db.LessonInfographics.Remove(info);
        }

        lesson.IsDeleted = true;
        lesson.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(int moduleId, List<SortOrderItem> items, int userId, bool isAdmin = false)
    {
        var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy chương.");

        if(!isAdmin && module.Course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý bài học cho khóa học này.");

        foreach(var item in items)
        {
            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == item.Id && l.ModuleId == moduleId);
            if(lesson != null)
                lesson.SortOrder = item.SortOrder;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<string> UploadAttachmentAsync(
        int moduleId,
        int lessonId,
        Stream fileStream,
        string fileName,
        int userId,
        bool isAdmin = false)
    {
        var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy chương.");

        if(!isAdmin && module.Course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý bài học cho khóa học này.");

        var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == lessonId && l.ModuleId == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy bài học trong chương này.");

        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var uploadsDir = Path.Combine(wwwroot, "uploads", "attachments");
        if(!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(fileName);
        var newFileName = $"{lessonId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, newFileName);

        using(var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            await fileStream.CopyToAsync(fs);

        var attachment = new LessonAttachment
        {
            LessonId = lessonId,
            FileName = fileName,
            StorageKey = $"attachments/{newFileName}",
            FileSize = fileStream.Length
        };
        _db.LessonAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        BackgroundJob.Enqueue<IAiProcessingService>(x => x.ProcessLessonAttachmentAsync(attachment.Id));

        return $"/uploads/attachments/{newFileName}";
    }

    public async Task<string> UploadVideoAsync(int lessonId, Stream fileStream, string fileName)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId) ?? throw new KeyNotFoundException("Không tìm thấy bài học.");

        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var videosDir = Path.Combine(wwwroot, "uploads", "videos");
        if(!Directory.Exists(videosDir))
            Directory.CreateDirectory(videosDir);

        var ext = Path.GetExtension(fileName);
        var newFileName = $"video_{lessonId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(videosDir, newFileName);

        using(var fs = new FileStream(filePath, FileMode.Create))
            await fileStream.CopyToAsync(fs);

        lesson.VideoProvider = "Local";
        lesson.VideoStorageKey = $"videos/{newFileName}";
        lesson.VideoStorageUrl = $"/uploads/videos/{newFileName}";
        lesson.VideoUrl = null;

        await _db.SaveChangesAsync();
        return lesson.VideoStorageUrl;
    }

    public async Task UpdateVideoMetadataAsync(int lessonId, string storageKey, string storageUrl)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId) ?? throw new KeyNotFoundException("Không tìm thấy bài học.");

        lesson.VideoProvider = "Local";
        lesson.VideoStorageKey = storageKey;
        lesson.VideoStorageUrl = storageUrl;
        lesson.VideoUrl = null;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAttachmentAsync(int moduleId, int attachmentId, int userId, bool isAdmin = false)
    {
        var module = await _db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy chương.");

        if(!isAdmin && module.Course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền quản lý bài học cho khóa học này.");

        var attachment = await _db.LessonAttachments
                .Include(a => a.Lesson)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Lesson.ModuleId == moduleId) ??
            throw new KeyNotFoundException("Không tìm thấy tệp đính kèm trong chương này.");

        await _vectorDb.DeleteVectorsByFilterAsync("AttachmentId", attachmentId);

        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task NotifyEnrolledUsersAsync(int lessonId, string type)
    {
        var lesson = await _db.Lessons
            .Include(l => l.Module)
            .ThenInclude(m => m.Course)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if(lesson is null || !lesson.Module.Course.IsPublished)
        {
            return;
        }

        var courseId = lesson.Module.CourseId;
        var courseTitle = lesson.Module.Course.Title;
        var lessonTitle = lesson.Title;

        var enrolledUserIds = await _db.Enrollments
            .Where(e => e.CourseId == courseId && (e.Status == "Approved" || e.Status == "Completed") && !e.IsDeleted)
            .Select(e => e.UserId)
            .Distinct()
            .ToListAsync();

        if(!enrolledUserIds.Any())
        {
            return;
        }

        var title = type is "NewLesson" ? "Bài giảng mới" : "Bài giảng đã được cập nhật";
        var message = type is "NewLesson"
            ? $"Khóa học '{courseTitle}' vừa có bài giảng mới: {lessonTitle}"
            : $"Bài giảng '{lessonTitle}' trong khóa học '{courseTitle}' vừa được cập nhật nội dung mới.";
        var link = $"/courses/{courseId}/learn?lessonId={lessonId}";

        foreach(var userId in enrolledUserIds)
        {
            await _notificationService.CreateNotificationAsync(userId, title, message, link, type);
        }
    }
}
