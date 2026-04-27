using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace LMS.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly VectorDbService _vectorDb;

    private readonly IConfiguration _config;

    public CourseService(
        AppDbContext db,
        INotificationService notificationService,
        VectorDbService vectorDb,
        IConfiguration config)
    {
        _db = db;
        _notificationService = notificationService;
        _vectorDb = vectorDb;
        _config = config;
    }

    public async Task<PagedResponse<CourseResponse>> GetCoursesAsync(
        int page,
        int pageSize,
        string? search,
        bool? isPublished = null,
        int? categoryId = null,
        int? userId = null,
        int? level = null,
        bool isInstructorManagement = false,
        bool isAdmin = false,
        int? userGroupId = null)
    {
        var query = _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category)
            .Include(c => c.UserGroup)
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        if(isInstructorManagement && userId.HasValue && !isAdmin)
        {
            query = query.Where(c => c.CreatedById == userId.Value);
        }

        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

        if(isPublished.HasValue)
            query = query.Where(c => c.IsPublished == isPublished.Value);

        if(categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(c => c.CategoryId == categoryId.Value);

        if(level.HasValue)
        {
            query = query.Where(c => c.Level == level.Value);
        }

        if(userGroupId.HasValue && userGroupId.Value > 0)
        {
            query = query.Where(c => c.UserGroupId == userGroupId.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(
                c => new CourseResponse(
                    c.Id,
                    c.Title,
                    c.Description,
                    c.CoverImageUrl,
                    c.IsPublished,
                    c.PassScore,
                    c.CreatedBy.FullName,
                    c.CreatedAt,
                    c.Modules.Count,
                    c.Modules.SelectMany(m => m.Lessons).Count(),
                    c.Enrollments.Count,
                    c.EnrollStartDate,
                    c.EnrollEndDate,
                    c.CompletionDeadlineDays,
                    c.CompletionEndDate,
                    c.RequiresApproval,
                    _db.Quizzes
                        .Where(q => q.CourseId == c.Id && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
                        .OrderByDescending(q => q.CreatedAt)
                        .Select(q => (int?)q.Id)
                        .FirstOrDefault(),
                    c.CategoryId,
                    c.Category != null ? c.Category.Name : null,
                    c.Level,
                    c.QuizRetakeWaitTimeMinutes,
                    c.QuizMaxRetakesPerDay,
                    c.UserGroupId,
                    c.UserGroup != null ? c.UserGroup.Name : null))
            .ToListAsync();

        return new PagedResponse<CourseResponse>(items, total, page, pageSize);
    }

    public async Task<CourseDetailResponse?> GetCourseDetailAsync(int courseId, int userId, bool isAdmin = false)
    {
        var course = await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category)
            .Include(c => c.UserGroup)
            .Include(c => c.Modules.OrderBy(m => m.SortOrder))
            .ThenInclude(m => m.Lessons.OrderBy(l => l.SortOrder))
            .ThenInclude(l => l.Attachments)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ThenInclude(l => l.RolePlayConfig)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ThenInclude(l => l.EssayConfig)
            .ThenInclude(c => c!.Questions)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ThenInclude(l => l.Quiz)
            .ThenInclude(q => q.Questions)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if(course == null)
            return null;

        var user = await _db.Users.FindAsync(userId);
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(
                e => e.UserId == userId && e.CourseId == courseId && (e.Status == "Approved" || e.Status == "Completed"));

        bool isEnrolled = enrollment != null;

        if(isEnrolled && enrollment!.GroupEnrollId.HasValue && !isAdmin)
        {
            var isStillInDept = await _db.UserGroupMembers
                .AnyAsync(m => m.UserId == userId && m.GroupId == enrollment.GroupEnrollId.Value);
            if(!isStillInDept)
                isEnrolled = false;
        }

        var quizCounts = await _db.QuestionBanks
            .Where(qb => !qb.IsDeleted && _db.Quizzes.Any(q => q.Id == qb.QuizId && q.CourseId == courseId))
            .GroupBy(qb => qb.QuizId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        var quizIdsWithQuestions = quizCounts.Keys.ToList();
        var extraQuizzes = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .Where(q => quizIdsWithQuestions.Contains(q.Id))
            .OrderBy(q => q.CreatedAt)
            .Select(q => new QuizSummaryResponse(q.Id, q.Title, quizCounts.GetValueOrDefault(q.Id)))
            .ToListAsync();

        var finalQuizId = extraQuizzes.LastOrDefault()?.Id;

        return new CourseDetailResponse(
            course.Id,
            course.Title,
            course.Description,
            course.CoverImageUrl,
            course.IsPublished,
            course.PassScore,
            course.CreatedBy.FullName,
            course.CreatedAt,
            course.Modules
                .Select(
                    m => new ModuleResponse(
                            m.Id,
                            m.Title,
                            m.SortOrder,
                            m.Lessons
                                .Where(
                                    l => l.Type != "Quiz" ||
                                                    (l.QuizId.HasValue &&
                                                        quizCounts.GetValueOrDefault(l.QuizId.Value) > 0))
                                .OrderBy(l => l.SortOrder)
                                .Select(
                                    l => new LessonResponse(
                                                    l.Id,
                                                    l.Title,
                                                    l.Type,
                                                    (isEnrolled || isAdmin || l.IsFreePreview) ? l.Content : null,
                                                    (isEnrolled || isAdmin || l.IsFreePreview)
                                                        ? (l.VideoStorageUrl ?? l.VideoUrl)
                                                        : null,
                                                    l.VideoDurationSeconds,
                                                    l.ReadingDurationSeconds,
                                                    l.VideoStatus,
                                                    l.SortOrder,
                                                    l.IsFreePreview,
                                                    (isEnrolled || isAdmin)
                                                        ? l.Attachments
                                                            .Select(
                                                                a => new AttachmentResponse(
                                                                                            a.Id,
                                                                                            a.FileName,
                                                                                            a.FileSize))
                                                            .ToList()
                                                        : new List<AttachmentResponse>(),
                                                    l.QuizId,
                                                    l.QuizId.HasValue ? quizCounts.GetValueOrDefault(l.QuizId.Value) : 0,
                                                    l.AiSummary,
                                                    l.VideoDraftScript,
                                                    l.RolePlayConfig != null
                                                        ? new RolePlayConfigDto(
                                                            JsonSerializer.Deserialize<List<int>>(
                                                                    l.RolePlayConfig.SupportLessonIds ?? "[]") ??
                                                                new List<int>(),
                                                            l.RolePlayConfig.ScoringCriteria,
                                                            l.RolePlayConfig.AdditionalRequirements,
                                                            l.RolePlayConfig.Scenario,
                                                            l.RolePlayConfig.PassScore)
                                                        : null,
                                                    l.EssayConfig != null
                                                        ? new EssayConfigDto(
                                                            JsonSerializer.Deserialize<List<int>>(
                                                                    l.EssayConfig.SupportLessonIds ?? "[]") ??
                                                                new List<int>(),
                                                            l.EssayConfig.TimeLimitMinutes,
                                                            l.EssayConfig.MaxAttemptsPerWindow,
                                                            l.EssayConfig.AttemptWindowHours,
                                                            l.EssayConfig.PassScore,
                                                            l.EssayConfig.Questions
                                                                .Where(q => !q.IsDeleted)
                                                                .Select(
                                                                    q => new EssayQuestionDto(
                                                                                                    q.Id,
                                                                                                    q.Content,
                                                                                                    q.SortOrder,
                                                                                                    q.Weight,
                                                                                                    q.ScoringCriteria))
                                                                .ToList())
                                                        : null))
                                .ToList()))
                .ToList(),
            await _db.Enrollments.CountAsync(e => e.CourseId == courseId),
            course.EnrollStartDate,
            course.EnrollEndDate,
            course.CompletionDeadlineDays,
            course.CompletionEndDate,
            course.RequiresApproval,
            finalQuizId,
            course.CategoryId,
            course.Category?.Name,
            course.Level,
            extraQuizzes,
            course.QuizRetakeWaitTimeMinutes,
            course.QuizMaxRetakesPerDay,
            course.UserGroupId,
            course.UserGroup?.Name);
    }

    public async Task<CourseDetailResponse?> GetCoursePreviewAsync(int courseId, int? userId = null)
    {
        var course = await _db.Courses

            .Include(c => c.CreatedBy)
            .Include(c => c.Category)
            .Include(c => c.UserGroup)
            .Include(c => c.Modules.OrderBy(m => m.SortOrder))
            .ThenInclude(m => m.Lessons.OrderBy(l => l.SortOrder))
            .ThenInclude(l => l.RolePlayConfig)
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .ThenInclude(l => l.EssayConfig)
            .ThenInclude(c => c!.Questions)
            .Where(c => c.Id == courseId && c.IsPublished && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if(course == null)
            return null;


        return new CourseDetailResponse(
            course.Id,
            course.Title,
            course.Description,
            course.CoverImageUrl,
            course.IsPublished,
            course.PassScore,
            course.CreatedBy.FullName,
            course.CreatedAt,
            course.Modules
                .Select(
                    m => new ModuleResponse(
                            m.Id,
                            m.Title,
                            m.SortOrder,
                            m.Lessons
                                .Select(
                                    l => new LessonResponse(
                                                    l.Id,
                                                    l.Title,
                                                    l.Type,
                                                    l.IsFreePreview ? l.Content : null,
                                                    l.IsFreePreview ? (l.VideoStorageUrl ?? l.VideoUrl) : null,
                                                    l.VideoDurationSeconds,
                                                    l.ReadingDurationSeconds,
                                                    l.VideoStatus,
                                                    l.SortOrder,
                                                    l.IsFreePreview,
                                                    new List<AttachmentResponse>(),
                                                    l.QuizId,
                                                    l.Quiz?.Questions.Count ?? 0,
                                                    l.AiSummary,
                                                    null,
                                                    l.RolePlayConfig != null
                                                        ? new RolePlayConfigDto(
                                                            JsonSerializer.Deserialize<List<int>>(
                                                                    l.RolePlayConfig.SupportLessonIds ?? "[]") ??
                                                                new List<int>(),
                                                            l.RolePlayConfig.ScoringCriteria,
                                                            l.RolePlayConfig.AdditionalRequirements,
                                                            l.RolePlayConfig.Scenario,
                                                            l.RolePlayConfig.PassScore)
                                                        : null,
                                                    l.EssayConfig != null
                                                        ? new EssayConfigDto(
                                                            JsonSerializer.Deserialize<List<int>>(
                                                                    l.EssayConfig.SupportLessonIds ?? "[]") ??
                                                                new List<int>(),
                                                            l.EssayConfig.TimeLimitMinutes,
                                                            l.EssayConfig.MaxAttemptsPerWindow,
                                                            l.EssayConfig.AttemptWindowHours,
                                                            l.EssayConfig.PassScore,
                                                            l.EssayConfig.Questions
                                                                .Where(q => !q.IsDeleted)
                                                                .Select(
                                                                    q => new EssayQuestionDto(
                                                                                                    q.Id,
                                                                                                    q.Content,
                                                                                                    q.SortOrder,
                                                                                                    q.Weight,
                                                                                                    q.ScoringCriteria))
                                                                .ToList())
                                                        : null))
                                .ToList()))
                .ToList(),
            await _db.Enrollments.CountAsync(e => e.CourseId == courseId),
            course.EnrollStartDate,
            course.EnrollEndDate,
            course.CompletionDeadlineDays,
            course.CompletionEndDate,
            course.RequiresApproval,
            _db.Quizzes
                .Where(q => q.CourseId == course.Id && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => (int?)q.Id)
                .FirstOrDefault(),
            course.CategoryId,
            course.Category?.Name,
            course.Level,
            await _db.Quizzes
                .Where(q => q.CourseId == course.Id && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
                .Select(q => new QuizSummaryResponse(q.Id, q.Title, q.Questions.Count))
                .ToListAsync(),
            course.QuizRetakeWaitTimeMinutes,
            course.QuizMaxRetakesPerDay,
            course.UserGroupId,
            course.UserGroup?.Name);
    }

    public async Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request, int userId)
    {
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            PassScore = request.PassScore,
            CreatedById = userId,
            EnrollStartDate = request.EnrollStartDate,
            EnrollEndDate = request.EnrollEndDate,
            CompletionDeadlineDays = request.CompletionDeadlineDays,
            CompletionEndDate = request.CompletionEndDate,
            CategoryId = request.CategoryId,
            Level = request.Level,
            IsPublished = request.IsPublished,
            QuizRetakeWaitTimeMinutes = request.QuizRetakeWaitTimeMinutes,
            QuizMaxRetakesPerDay = request.QuizMaxRetakesPerDay,
            UserGroupId = request.UserGroupId
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        if(course.IsPublished)
        {
            await NotifyNewCourseAsync(course);
        }

        var user = await _db.Users.FindAsync(userId);
        var categoryName = course.CategoryId.HasValue ? (await _db.Categories.FindAsync(course.CategoryId))?.Name : null;

        return new CourseResponse(
            course.Id,
            course.Title,
            course.Description,
            course.CoverImageUrl,
            course.IsPublished,
            course.PassScore,
            user!.FullName,
            course.CreatedAt,
            0,
            0,
            0,
            course.EnrollStartDate,
            course.EnrollEndDate,
            course.CompletionDeadlineDays,
            course.CompletionEndDate,
            course.RequiresApproval,
            null,
            course.CategoryId,
            categoryName,
            course.Level,
            course.QuizRetakeWaitTimeMinutes,
            course.QuizMaxRetakesPerDay,
            course.UserGroupId,
            null);
    }

    public async Task<CourseResponse> UpdateCourseAsync(
        int courseId,
        UpdateCourseRequest request,
        int userId,
        bool isAdmin = false)
    {
        var course = await _db.Courses.Include(c => c.CreatedBy).FirstOrDefaultAsync(c => c.Id == courseId) ??
            throw new KeyNotFoundException("Không tìm thấy khóa học.");

        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền chỉnh sửa khóa học này.");

        course.Title = request.Title;
        course.Description = request.Description;
        course.PassScore = request.PassScore;
        course.IsPublished = request.IsPublished;
        course.EnrollStartDate = request.EnrollStartDate;
        course.EnrollEndDate = request.EnrollEndDate;
        course.CompletionDeadlineDays = request.CompletionDeadlineDays;
        course.CompletionEndDate = request.CompletionEndDate;
        course.UpdatedAt = DateTime.UtcNow;
        course.CategoryId = request.CategoryId;
        course.Level = request.Level;
        course.QuizRetakeWaitTimeMinutes = request.QuizRetakeWaitTimeMinutes;
        course.QuizMaxRetakesPerDay = request.QuizMaxRetakesPerDay;
        course.UserGroupId = request.UserGroupId;

        bool wasPublished = (bool)(_db.Entry(course).OriginalValues["IsPublished"] ?? false);
        await _db.SaveChangesAsync();

        if(!wasPublished && course.IsPublished)
        {
            await NotifyNewCourseAsync(course);
        }

        var finalQuizId = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => (int?)q.Id)
            .FirstOrDefaultAsync();

        var categoryName = course.CategoryId.HasValue ? (await _db.Categories.FindAsync(course.CategoryId))?.Name : null;

        return new CourseResponse(
            course.Id,
            course.Title,
            course.Description,
            course.CoverImageUrl,
            course.IsPublished,
            course.PassScore,
            course.CreatedBy.FullName,
            course.CreatedAt,
            await _db.Modules.CountAsync(m => m.CourseId == courseId),
            await _db.Lessons.CountAsync(l => l.Module.CourseId == courseId),
            await _db.Enrollments.CountAsync(e => e.CourseId == courseId),
            course.EnrollStartDate,
            course.EnrollEndDate,
            course.CompletionDeadlineDays,
            course.CompletionEndDate,
            course.RequiresApproval,
            finalQuizId,
            course.CategoryId,
            categoryName,
            course.Level,
            course.QuizRetakeWaitTimeMinutes,
            course.QuizMaxRetakesPerDay,
            course.UserGroupId,
            course.UserGroup?.Name);
    }

    public async Task DeleteCourseAsync(int courseId, int userId, bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền xóa khóa học này.");

        var lessonIds = await _db.Lessons.Where(l => l.Module.CourseId == courseId).Select(l => l.Id).ToListAsync();

        foreach(var lessonId in lessonIds)
        {
            await _vectorDb.DeleteVectorsByFilterAsync("LessonId", lessonId);
        }

        course.IsDeleted = true;
        course.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<string> UploadCoverImageAsync(
        int courseId,
        Stream fileStream,
        string fileName,
        int userId,
        bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền tải lên ảnh bìa cho khóa học này.");

        var storageRoot = _config["Storage:RootPath"];
        var wwwroot = string.IsNullOrEmpty(storageRoot)
            ? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
            : storageRoot;

        var uploadsDir = Path.Combine(wwwroot, "uploads", "covers");
        if(!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);
        var ext = Path.GetExtension(fileName);
        var newFileName = $"{courseId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, newFileName);
        using(var fs = new FileStream(filePath, FileMode.Create))
            await fileStream.CopyToAsync(fs);
        course.CoverImageUrl = $"/uploads/covers/{newFileName}";
        await _db.SaveChangesAsync();
        return course.CoverImageUrl;
    }

    public async Task<CertificateTemplateDto?> GetCourseCertificateTemplateAsync(int courseId)
    {
        var template = await _db.CertificateTemplates.FirstOrDefaultAsync(t => t.CourseId == courseId);
        if(template == null)
            return null;
        return new CertificateTemplateDto(
            template.UseBuiltInTemplate,
            template.HtmlTemplate,
            template.BackgroundImageUrl,
            template.LogoUrl,
            template.SignatureImageUrl,
            template.SignatureName,
            template.SignatureTitle,
            template.TitleText,
            template.BodyText,
            template.FooterText,
            template.PrimaryColor);
    }

    public async Task<CertificateTemplateDto> SaveCourseCertificateTemplateAsync(
        int courseId,
        CertificateTemplateDto request,
        int userId,
        bool isAdmin = false)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Khóa học không tồn tại.");
        if(!isAdmin && course.CreatedById != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền thay đổi mẫu chứng chỉ của khóa học này.");

        var template = await _db.CertificateTemplates.FirstOrDefaultAsync(t => t.CourseId == courseId);
        if(template == null)
        {
            template = new CertificateTemplate { CourseId = courseId };
            _db.CertificateTemplates.Add(template);
        }
        template.UseBuiltInTemplate = request.UseBuiltInTemplate;
        template.HtmlTemplate = request.HtmlTemplate;
        template.BackgroundImageUrl = request.BackgroundImageUrl;
        template.LogoUrl = request.LogoUrl;
        template.SignatureImageUrl = request.SignatureImageUrl;
        template.SignatureName = request.SignatureName;
        template.SignatureTitle = request.SignatureTitle;
        template.TitleText = request.TitleText;
        template.BodyText = request.BodyText;
        template.FooterText = request.FooterText;
        template.PrimaryColor = request.PrimaryColor;
        await _db.SaveChangesAsync();
        return request;
    }

    private async Task NotifyNewCourseAsync(Course course)
    {
        var usersQuery = _db.Users.Where(u => u.IsActive && !u.IsDeleted);

        if(course.UserGroupId.HasValue)
        {
            usersQuery = usersQuery.Where(
                u => _db.UserGroupMembers.Any(m => m.UserId == u.Id && m.GroupId == course.UserGroupId.Value));
        }

        var users = await usersQuery.ToListAsync();

        foreach(var user in users)
        {
            await _notificationService.CreateNotificationAsync(
                user.Id,
                "Khóa học mới vừa ra mắt",
                $"Khóa học '{course.Title}' đã được xuất bản. Hãy khám phá ngay!",
                $"/courses/{course.Id}",
                "NewCourse");
        }
    }
}
