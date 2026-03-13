using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _db;
    public EnrollmentService(AppDbContext db) => _db = db;

    public async Task<EnrollmentResponse> EnrollAsync(int userId, int courseId)
    {
        var existing = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (existing != null) throw new InvalidOperationException("Bạn đã ghi danh khóa học này.");

        // Validate thời hạn đăng ký
        var course = await _db.Courses.FindAsync(courseId)
            ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        var now = DateTime.UtcNow;
        if (course.EnrollStartDate != null && now < course.EnrollStartDate)
            throw new InvalidOperationException($"Khóa học chưa mở đăng ký. Bắt đầu từ {course.EnrollStartDate:dd/MM/yyyy}.");
        if (course.EnrollEndDate != null && now > course.EnrollEndDate)
            throw new InvalidOperationException($"Đã hết thời hạn đăng ký (hạn chót: {course.EnrollEndDate:dd/MM/yyyy}).");

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            Status = course.RequiresApproval ? "Pending" : "Approved"
        };
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();
        return await MapEnrollmentAsync(enrollment);
    }

    public async Task<EnrollmentResponse> AdminEnrollAsync(AdminEnrollRequest request, int assignedById)
    {
        var existing = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == request.UserId && e.CourseId == request.CourseId);
        if (existing != null) throw new InvalidOperationException("Nhân viên đã ghi danh khóa học này.");

        var enrollment = new Enrollment
        {
            UserId = request.UserId,
            CourseId = request.CourseId,
            Status = "Approved",
            Deadline = request.Deadline,
            IsMandatory = request.IsMandatory,
            AssignedById = assignedById
        };
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();
        return await MapEnrollmentAsync(enrollment);
    }

    public async Task<EnrollmentResponse> ApproveEnrollmentAsync(int enrollmentId, bool approved)
    {
        var enrollment = await _db.Enrollments.FindAsync(enrollmentId)
            ?? throw new KeyNotFoundException("Không tìm thấy ghi danh.");

        enrollment.Status = approved ? "Approved" : "Rejected";
        await _db.SaveChangesAsync();
        return await MapEnrollmentAsync(enrollment);
    }

    public async Task<List<EnrollmentResponse>> GetUserEnrollmentsAsync(int userId)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.User).Include(e => e.Course)
            .Where(e => e.UserId == userId && (e.Status == "Approved" || e.Status == "Pending"))
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var result = new List<EnrollmentResponse>();
        foreach (var e in enrollments)
            result.Add(await MapEnrollmentAsync(e));
        return result;
    }

    public async Task<List<EnrollmentResponse>> GetCourseEnrollmentsAsync(int courseId)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.User).Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var result = new List<EnrollmentResponse>();
        foreach (var e in enrollments)
            result.Add(await MapEnrollmentAsync(e));
        return result;
    }

    public async Task<List<EnrollmentResponse>> GetPendingEnrollmentsAsync()
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.User).Include(e => e.Course)
            .Where(e => e.Status == "Pending")
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var result = new List<EnrollmentResponse>();
        foreach (var e in enrollments)
            result.Add(await MapEnrollmentAsync(e));
        return result;
    }

    private async Task<EnrollmentResponse> MapEnrollmentAsync(Enrollment e)
    {
        await _db.Entry(e).Reference(x => x.User).LoadAsync();
        await _db.Entry(e).Reference(x => x.Course).LoadAsync();

        var totalLessons = await _db.Lessons.CountAsync(l => l.Module.CourseId == e.CourseId);
        var completedLessons = await _db.LessonProgresses
            .CountAsync(lp => lp.UserId == e.UserId && lp.IsCompleted && lp.Lesson.Module.CourseId == e.CourseId);

        var progress = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;
        var isOverdue = e.Deadline.HasValue && e.Deadline.Value < DateTime.UtcNow && progress < 100;

        // Truyền thêm TotalLessons và CompletedLessons vào
        return new EnrollmentResponse(
            e.Id, e.UserId, e.User.FullName, e.User.Avatar, e.CourseId, e.Course.Title,
            e.Status, e.Deadline, e.IsMandatory, e.EnrolledAt, Math.Round(progress, 1), isOverdue,
            totalLessons, completedLessons);
    }

    // ĐÂY LÀ HÀM ĐÃ ĐƯỢC TỐI ƯU HÓA (CHỐNG LỖI 500)
    public async Task<object> GetAllEnrollmentsAsync(int page, int pageSize)
    {
        var query = _db.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
            .Where(e => !e.IsDeleted);

        var total = await query.CountAsync();

        var enrollments = await query
            .OrderByDescending(e => e.EnrolledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 1. TỐI ƯU HÓA: Lấy toàn bộ tiến độ lên RAM 1 lần duy nhất thay vì chọc DB trong vòng lặp
        var userIds = enrollments.Select(e => e.UserId).Distinct().ToList();
        var allProgresses = await _db.LessonProgresses
            .Where(lp => userIds.Contains(lp.UserId) && lp.IsCompleted && !lp.IsDeleted)
            .ToListAsync();

        var items = new List<EnrollmentProgressResponse>();

        foreach (var e in enrollments)
        {
            // Bắt lỗi an toàn nếu Course hoặc Modules bị null
            if (e.Course == null) continue;

            var lessonIds = e.Course.Modules != null
                ? e.Course.Modules.SelectMany(m => m.Lessons ?? new List<Lesson>()).Select(l => l.Id).ToList()
                : new List<int>();

            var totalLessons = lessonIds.Count;

            // Đếm số bài đã học bằng List trên RAM (Cực nhanh)
            var completedLessons = allProgresses.Count(lp => lp.UserId == e.UserId && lessonIds.Contains(lp.LessonId));

            double progress = totalLessons == 0 ? 0 : Math.Round(((double)completedLessons / totalLessons) * 100, 2);

            string currentStatus = string.IsNullOrEmpty(e.Status) ? "Pending" : e.Status;

            items.Add(new EnrollmentProgressResponse(e.UserId, e.CourseId, currentStatus, progress));
        }

        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}