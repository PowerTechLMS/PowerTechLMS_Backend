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

    // HÀM LẤY DANH SÁCH (ĐÃ GỘP ĐẦY ĐỦ TÍNH NĂNG LỌC VÀ PHÂN QUYỀN PHÒNG BAN)
    public async Task<PagedResponse<CourseResponse>> GetCoursesAsync(int page, int pageSize, string? search, bool? isPublished = null, int? categoryId = null, int? userId = null, int? level = null)
    {
        var query = _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted) // Đảm bảo lọc IsDeleted ngay từ đầu
            .AsQueryable();

        // 1. Lọc theo search, category, published như cũ
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

        if (isPublished.HasValue)
            query = query.Where(c => c.IsPublished == isPublished.Value);

        if (categoryId.HasValue && categoryId.Value > 0)
            query = query.Where(c => c.CategoryId == categoryId.Value);

        if (level.HasValue)
        {
            query = query.Where(c => c.Level == level.Value);
            
            // [MỚI] Nếu là Level 3 (Khám phá) và có userId -> Loại trừ các khoá ĐÃ ĐƯỢC PHÉP VÀO
            if (level.Value == 3 && userId.HasValue)
            {
                // 1. Loại trừ các khoá đã có trong Enrollments
                var enrolledCourseIds = await _db.Enrollments
                    .Where(e => e.UserId == userId.Value && !e.IsDeleted)
                    .Select(e => e.CourseId)
                    .ToListAsync();
                
                // 2. Loại trừ các khoá thuộc lộ trình các phòng ban mà user tham gia
                var deptCourseIds = await _db.UserGroupMembers
                    .Where(m => m.UserId == userId.Value && !m.IsDeleted)
                    .Join(_db.DepartmentCourseGroups, m => m.GroupId, dcg => dcg.DepartmentId, (m, dcg) => dcg.CourseGroupId)
                    .Join(_db.CourseGroupCourses.Where(cgc => !cgc.IsDeleted), cgid => cgid, cgc => cgc.GroupId, (cgid, cgc) => cgc.CourseId)
                    .Distinct()
                    .ToListAsync();

                var excludedIds = enrolledCourseIds.Union(deptCourseIds).ToList();
                query = query.Where(c => !excludedIds.Contains(c.Id));
            }
        }

        // 2. [QUAN TRỌNG] Lọc theo tiến độ và phòng ban (Nếu không phải Admin)
        if (userId.HasValue)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.Role != "Admin")
            {
                // Kiểm tra xem đã hoàn thành toàn bộ Level 1 chưa
                var level1CourseIds = await _db.Courses
                    .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
                    .Select(c => c.Id)
                    .ToListAsync();

                bool isLevel1Completed = true;
                if (level1CourseIds.Any())
                {
                    var completedCount = await _db.Enrollments
                        .CountAsync(e => e.UserId == userId.Value && level1CourseIds.Contains(e.CourseId) && e.Status == "Completed");
                    isLevel1Completed = completedCount >= level1CourseIds.Count;
                }

                // Nếu chưa xong Level 1 -> Ẩn toàn bộ Level 2
                if (!isLevel1Completed)
                {
                    query = query.Where(c => c.Level != 2);
                }
                else
                {
                    // Nếu đã xong Level 1 -> Chỉ hiện Level 2 nếu thuộc phòng ban hoặc đã ghi danh
                    var myCourseGroupIds = await _db.UserGroupMembers
                        .Where(m => m.UserId == userId)
                        .Join(_db.DepartmentCourseGroups, m => m.GroupId, dcg => dcg.DepartmentId, (m, dcg) => dcg.CourseGroupId)
                        .ToListAsync();

                    var myEnrolledCourseIds = await _db.Enrollments
                        .Where(e => e.UserId == userId && !e.IsDeleted)
                        .Select(e => e.CourseId)
                        .ToListAsync();

                    query = query.Where(c =>
                        c.Level != 2 || 
                        myEnrolledCourseIds.Contains(c.Id) || 
                        _db.CourseGroupCourses.Any(cgi => cgi.CourseId == c.Id && myCourseGroupIds.Contains(cgi.GroupId))
                    );
                }
            }
        }
        else
        {
            // Anonymous chỉ thấy Level 1 và Level 3
            query = query.Where(c => c.Level != 2);
        }

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
                c.Category != null ? c.Category.Name : null,
                c.Level
            ))
            .ToListAsync();

        return new PagedResponse<CourseResponse>(items, total, page, pageSize);
    }

    public async Task<CourseDetailResponse?> GetCourseDetailAsync(int courseId, int userId)
    {
        var course = await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category) // Phải có để lấy CategoryName
            .Include(c => c.Modules.OrderBy(m => m.SortOrder))
                .ThenInclude(m => m.Lessons.OrderBy(l => l.SortOrder))
                    .ThenInclude(l => l.Attachments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null) return null;

        // [MỚI] Kiểm tra ghi danh để ẩn nội dung nếu chưa học
        var user = await _db.Users.FindAsync(userId);
        var isAdmin = user?.Role == "Admin";
        
        // [MỚI] Chặn truy cập Level 2 nếu chưa xong Level 1
        if (course.Level == 2 && !isAdmin)
        {
            var level1Ids = await _db.Courses.Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished).Select(c => c.Id).ToListAsync();
            if (level1Ids.Any())
            {
                var completedCount = await _db.Enrollments.CountAsync(e => e.UserId == userId && level1Ids.Contains(e.CourseId) && e.Status == "Completed");
                if (completedCount < level1Ids.Count) return null; // Ẩn hoàn toàn nếu chưa đạt điều kiện
            }
        }

        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && (e.Status == "Approved" || e.Status == "Completed"));

        bool isEnrolled = enrollment != null;

        // Nếu ghi danh theo phòng ban -> Phải check xem còn ở trong phòng ban đó không
        if (isEnrolled && enrollment!.GroupEnrollId.HasValue && !isAdmin)
        {
             var isStillInDept = await _db.UserGroupMembers.AnyAsync(m => m.UserId == userId && m.GroupId == enrollment.GroupEnrollId.Value);
             if (!isStillInDept) isEnrolled = false; // Coi như chưa ghi danh nếu đã rời phòng ban
        }

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
                    l.Id, l.Title, l.Type, 
                    (isEnrolled || isAdmin || l.IsFreePreview) ? l.Content : null, // Ẩn content nếu chưa ghi danh
                    (isEnrolled || isAdmin || l.IsFreePreview) ? (l.VideoStorageUrl ?? l.VideoUrl) : null, // Ẩn video nếu chưa ghi danh
                    l.VideoDurationSeconds, l.SortOrder, l.IsFreePreview,
                    (isEnrolled || isAdmin) ? l.Attachments.Select(a => new AttachmentResponse(a.Id, a.FileName, a.FileSize)).ToList() : new List<AttachmentResponse>(),
                    l.QuizId
                )).ToList()
            )).ToList(),
            course.Enrollments?.Count ?? 0,
            course.EnrollStartDate, course.EnrollEndDate,
            course.CompletionDeadlineDays, course.CompletionEndDate,
            course.RequiresApproval, // Đối số 15
            finalQuizId,             // Đối số 16
            course.CategoryId,       // Đối số 17 (Mới)
            course.Category?.Name,   // Đối số 18 (Mới)
            course.Level             // Đối số 19 (Mới)
        );
    }

    public async Task<CourseDetailResponse?> GetCoursePreviewAsync(int courseId, int? userId = null)
    {
        var course = await _db.Courses
            .Include(c => c.CreatedBy)
            .Include(c => c.Category)
            .Include(c => c.Modules.OrderBy(m => m.SortOrder))
                .ThenInclude(m => m.Lessons.OrderBy(l => l.SortOrder))
            .Where(c => c.Id == courseId && c.IsPublished && !c.IsDeleted)
            .FirstOrDefaultAsync();

        if (course == null) return null;

        // Kiểm tra quyền xem preview (Nếu là Level 2 thì phải thuộc phòng ban hoặc là Admin)
        if (course.Level == 2)
        {
            var isAdmin = false;
            var isAllowed = false;

            if (userId.HasValue)
            {
                var user = await _db.Users.FindAsync(userId);
                isAdmin = user?.Role == "Admin";

                // [MỚI] Chặn preview Level 2 nếu chưa xong Level 1
                if (!isAdmin)
                {
                    var level1Ids = await _db.Courses.Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished).Select(c => c.Id).ToListAsync();
                    if (level1Ids.Any())
                    {
                        var completedCount = await _db.Enrollments.CountAsync(enroll => enroll.UserId == userId && level1Ids.Contains(enroll.CourseId) && enroll.Status == "Completed");
                        if (completedCount < level1Ids.Count) return null;
                    }
                }

                var myCourseGroupIds = await _db.UserGroupMembers
                   .Where(m => m.UserId == userId)
                   .Join(_db.DepartmentCourseGroups, m => m.GroupId, dcg => dcg.DepartmentId, (m, dcg) => dcg.CourseGroupId)
                   .ToListAsync();

                isAllowed = await _db.CourseGroupCourses.AnyAsync(cgi => cgi.CourseId == courseId && myCourseGroupIds.Contains(cgi.GroupId)) ||
                            await _db.Enrollments.AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
            }

            if (!isAdmin && !isAllowed) return null; // Không được xem cả preview nếu là khoá phòng ban khác
        }

        return new CourseDetailResponse(
            course.Id, course.Title, course.Description, course.CoverImageUrl,
            course.IsPublished, course.PassScore, course.CreatedBy.FullName, course.CreatedAt,
            course.Modules.Select(m => new ModuleResponse(
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
            await _db.Enrollments.CountAsync(e => e.CourseId == courseId),
            course.EnrollStartDate, course.EnrollEndDate,
            course.CompletionDeadlineDays, course.CompletionEndDate,
            course.RequiresApproval,
            _db.Quizzes.Where(q => q.CourseId == course.Id && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id)).OrderByDescending(q => q.CreatedAt).Select(q => (int?)q.Id).FirstOrDefault(),
            course.CategoryId,
            course.Category?.Name,
            course.Level
        );
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
            Level = request.Level
        };

        _db.Courses.Add(course);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        var categoryName = course.CategoryId.HasValue ? (await _db.Categories.FindAsync(course.CategoryId))?.Name : null;

        return new CourseResponse(course.Id, course.Title, course.Description, course.CoverImageUrl,
            course.IsPublished, course.PassScore, user!.FullName, course.CreatedAt, 0, 0, 0,
            course.EnrollStartDate, course.EnrollEndDate,
            course.CompletionDeadlineDays, course.CompletionEndDate,
            course.RequiresApproval, null, course.CategoryId, categoryName, course.Level);
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
        course.Level = request.Level;

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
            finalQuizId, course.CategoryId, categoryName, course.Level);
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