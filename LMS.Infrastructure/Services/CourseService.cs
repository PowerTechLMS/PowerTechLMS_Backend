using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class CourseService : ICourseService
{
    private readonly AppDbContext _db;

    public CourseService(AppDbContext db) => _db = db;

    // HÀM LẤY DANH SÁCH (ĐÃ GỘP ĐẦY ĐỦ TÍNH NĂNG LỌC)
    public async Task<PagedResponse<CourseResponse>> GetCoursesAsync(int page, int pageSize, string? search, bool? isPublished = null, int? categoryId = null)
    {
        var query = _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category) // Nối bảng lấy tên Category
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

        if (isPublished.HasValue)
            query = query.Where(c => c.IsPublished == isPublished.Value);

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(c => c.CategoryId == categoryId.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CourseResponse(
                c.Id, c.Title, c.Description, c.CoverImageUrl,
                c.IsPublished, c.PassScore, c.CreatedBy.FullName, c.CreatedAt,
                c.Modules.Count,
                c.Modules.SelectMany(m => m.Lessons).Count(),
                c.Enrollments.Count,
                c.EnrollStartDate, c.EnrollEndDate,
                c.CompletionDeadlineDays, c.CompletionEndDate,
                c.RequiresApproval,
                _db.Quizzes.Where(q => q.CourseId == c.Id && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id)).OrderByDescending(q => q.CreatedAt).Select(q => (int?)q.Id).FirstOrDefault(),
                c.CategoryId,
                c.Category != null ? c.Category.Name : null // Lấy tên danh mục
            ))
            .ToListAsync();

        return new PagedResponse<CourseResponse>(items, total, page, pageSize);
    }

    public async Task<CourseDetailResponse?> GetCourseDetailAsync(int courseId)
    {
        var course = await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category) // Phải có để lấy CategoryName
            .Include(c => c.Modules.OrderBy(m => m.SortOrder))
                .ThenInclude(m => m.Lessons.OrderBy(l => l.SortOrder))
                    .ThenInclude(l => l.Attachments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return null;

        var finalQuizId = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => (int?)q.Id)
            .FirstOrDefaultAsync();

        return new CourseDetailResponse(
            course.Id, course.Title, course.Description, course.CoverImageUrl,
            course.IsPublished, course.PassScore, course.CreatedBy.FullName, course.CreatedAt,
            course.Modules.Select(m => new ModuleResponse(
                m.Id, m.Title, m.SortOrder,
                m.Lessons.Select(l => new LessonResponse(
                    l.Id, l.Title, l.Type, l.Content, l.VideoStorageUrl ?? l.VideoUrl,
                    l.VideoDurationSeconds, l.SortOrder, l.IsFreePreview,
                    l.Attachments.Select(a => new AttachmentResponse(a.Id, a.FileName, a.FileSize)).ToList(),
                    l.QuizId
                )).ToList()
            )).ToList(),
            course.Enrollments?.Count ?? 0,
            course.EnrollStartDate, course.EnrollEndDate,
            course.CompletionDeadlineDays, course.CompletionEndDate,
            course.RequiresApproval, // Đối số 15
            finalQuizId,             // Đối số 16
            course.CategoryId,       // Đối số 17 (Mới)
            course.Category?.Name    // Đối số 18 (Mới)
        );
    }

    public async Task<CourseDetailResponse?> GetCoursePreviewAsync(int courseId)
    {
        return await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category)
            .Include(c => c.Modules.OrderBy(m => m.SortOrder))
                .ThenInclude(m => m.Lessons.OrderBy(l => l.SortOrder))
            .Where(c => c.Id == courseId && c.IsPublished)
            .Select(c => new CourseDetailResponse(
                c.Id, c.Title, c.Description, c.CoverImageUrl,
                c.IsPublished, c.PassScore, c.CreatedBy.FullName, c.CreatedAt,
                c.Modules.Select(m => new ModuleResponse(
                    m.Id, m.Title, m.SortOrder,
                    m.Lessons.Select(l => new LessonResponse(
                        l.Id, l.Title, l.Type,
                        l.IsFreePreview ? l.Content : null,
                        l.IsFreePreview ? (l.VideoStorageUrl ?? l.VideoUrl) : null,
                        l.VideoDurationSeconds, l.SortOrder, l.IsFreePreview,
                        new List<AttachmentResponse>(),
                        l.QuizId
                    )).ToList()
                )).ToList(),
                c.Enrollments.Count,
                c.EnrollStartDate, c.EnrollEndDate,
                c.CompletionDeadlineDays, c.CompletionEndDate,
                c.RequiresApproval,
                _db.Quizzes.Where(q => q.CourseId == c.Id && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id)).OrderByDescending(q => q.CreatedAt).Select(q => (int?)q.Id).FirstOrDefault(),
                c.CategoryId,
                c.Category != null ? c.Category.Name : null
            ))
            .FirstOrDefaultAsync();
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
            CategoryId = request.CategoryId
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        var categoryName = course.CategoryId.HasValue ? (await _db.Categories.FindAsync(course.CategoryId))?.Name : null;

        return new CourseResponse(course.Id, course.Title, course.Description, course.CoverImageUrl,
            course.IsPublished, course.PassScore, user!.FullName, course.CreatedAt, 0, 0, 0,
            course.EnrollStartDate, course.EnrollEndDate,
            course.CompletionDeadlineDays, course.CompletionEndDate,
            course.RequiresApproval, null, course.CategoryId, categoryName);
    }

    public async Task<CourseResponse> UpdateCourseAsync(int courseId, UpdateCourseRequest request)
    {
        var course = await _db.Courses.Include(c => c.CreatedBy).FirstOrDefaultAsync(c => c.Id == courseId)
            ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");

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

        await _db.SaveChangesAsync();

        var finalQuizId = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .OrderByDescending(q => q.CreatedAt)
            .Select(q => (int?)q.Id)
            .FirstOrDefaultAsync();

        var categoryName = course.CategoryId.HasValue ? (await _db.Categories.FindAsync(course.CategoryId))?.Name : null;

        return new CourseResponse(course.Id, course.Title, course.Description, course.CoverImageUrl,
            course.IsPublished, course.PassScore, course.CreatedBy.FullName, course.CreatedAt,
            await _db.Modules.CountAsync(m => m.CourseId == courseId),
            await _db.Lessons.CountAsync(l => l.Module.CourseId == courseId),
            await _db.Enrollments.CountAsync(e => e.CourseId == courseId),
            course.EnrollStartDate, course.EnrollEndDate,
            course.CompletionDeadlineDays, course.CompletionEndDate,
            course.RequiresApproval,
            finalQuizId, course.CategoryId, categoryName);
    }

    public async Task DeleteCourseAsync(int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        course.IsDeleted = true; course.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<string> UploadCoverImageAsync(int courseId, Stream fileStream, string fileName)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
        var ext = Path.GetExtension(fileName);
        var newFileName = $"{courseId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, newFileName);
        using (var fs = new FileStream(filePath, FileMode.Create)) await fileStream.CopyToAsync(fs);
        course.CoverImageUrl = $"/uploads/covers/{newFileName}";
        await _db.SaveChangesAsync();
        return course.CoverImageUrl;
    }

    public async Task<CertificateTemplateDto?> GetCourseCertificateTemplateAsync(int courseId)
    {
        var template = await _db.CertificateTemplates.FirstOrDefaultAsync(t => t.CourseId == courseId);
        if (template == null) return null;
        return new CertificateTemplateDto(template.UseBuiltInTemplate, template.HtmlTemplate, template.BackgroundImageUrl, template.LogoUrl, template.SignatureImageUrl, template.SignatureName, template.SignatureTitle, template.TitleText, template.BodyText, template.FooterText, template.PrimaryColor);
    }

    public async Task<CertificateTemplateDto> SaveCourseCertificateTemplateAsync(int courseId, CertificateTemplateDto request)
    {
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Khóa học không tồn tại.");
        var template = await _db.CertificateTemplates.FirstOrDefaultAsync(t => t.CourseId == courseId);
        if (template == null) { template = new CertificateTemplate { CourseId = courseId }; _db.CertificateTemplates.Add(template); }
        template.UseBuiltInTemplate = request.UseBuiltInTemplate; template.HtmlTemplate = request.HtmlTemplate; template.BackgroundImageUrl = request.BackgroundImageUrl; template.LogoUrl = request.LogoUrl; template.SignatureImageUrl = request.SignatureImageUrl; template.SignatureName = request.SignatureName; template.SignatureTitle = request.SignatureTitle; template.TitleText = request.TitleText; template.BodyText = request.BodyText; template.FooterText = request.FooterText; template.PrimaryColor = request.PrimaryColor;
        await _db.SaveChangesAsync();
        return request;
    }
}