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

        // [MỚI] Kiểm tra điều kiện Cấp 1 (Bắt buộc) nếu đăng ký Cấp 2
        if (course.Level == 2)
        {
            // 1. Phải thuộc phòng ban có gán khoá học này (Trừ khi được Admin gán lẻ - nhưng ở đây là user tự đăng ký)
            var allowedCourseGroupIds = await _db.CourseGroupCourses
                .Where(cgc => cgc.CourseId == courseId && !cgc.IsDeleted)
                .Select(cgc => cgc.GroupId)
                .ToListAsync();
            
            var userDepartmentIds = await _db.UserGroupMembers
                .Where(ugm => ugm.UserId == userId && !ugm.IsDeleted)
                .Select(ugm => ugm.GroupId)
                .ToListAsync();

            var isDepartmentMatch = await _db.DepartmentCourseGroups
                .AnyAsync(dcg => userDepartmentIds.Contains(dcg.DepartmentId) && allowedCourseGroupIds.Contains(dcg.CourseGroupId));

            if (!isDepartmentMatch)
            {
                throw new InvalidOperationException("Khoá học chuyên ngành này không thuộc phòng ban của bạn.");
            }

            // 2. Phải hoàn thành Level 1
            var level1CourseIds = await _db.Courses
                .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
                .Select(c => c.Id)
                .ToListAsync();

            if (level1CourseIds.Any())
            {
                var completedLevel1Count = await _db.Enrollments
                    .CountAsync(e => e.UserId == userId && level1CourseIds.Contains(e.CourseId) && e.Status == "Completed");

                if (completedLevel1Count < level1CourseIds.Count)
                {
                    throw new InvalidOperationException("Bạn cần hoàn thành TOÀN BỘ các khóa học Bắt buộc (Cấp 1) trước khi đăng ký khóa học này.");
                }
            }
        }

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
        // 1. Get all basic enrollments
        var enrollments = await _db.Enrollments
            .Include(e => e.User).Include(e => e.Course)
            .Where(e => e.UserId == userId && (e.Status == "Approved" || e.Status == "Pending" || e.Status == "Completed"))
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        // [AUTO-ENROLL LEVEL 1]: Đảm bảo tất cả user luôn được gán khoá Cấp 1 (Người mới)
        var level1CourseIds = await _db.Courses
            .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
            .Select(c => c.Id)
            .ToListAsync();

        if (level1CourseIds.Any())
        {
            var myLevel1EnrollmentCourseIds = enrollments
                .Where(e => e.Course.Level == 1)
                .Select(e => e.CourseId)
                .ToList();

            var missingLevel1Ids = level1CourseIds.Except(myLevel1EnrollmentCourseIds).ToList();

            if (missingLevel1Ids.Any())
            {
                foreach (var courseId in missingLevel1Ids)
                {
                    var newEnroll = new Enrollment
                    {
                        UserId = userId,
                        CourseId = courseId,
                        Status = "Approved",
                        IsMandatory = true,
                        EnrolledAt = DateTime.UtcNow,
                        ApprovedAt = DateTime.UtcNow
                    };
                    _db.Enrollments.Add(newEnroll);
                    
                    // Thêm vào list local để render luôn không cần fetch lại
                    await _db.Entry(newEnroll).Reference(x => x.Course).LoadAsync();
                    enrollments.Add(newEnroll);
                }
                await _db.SaveChangesAsync();
            }
        }

        // 2. Fetch user's current department memberships
        var myDepartmentIds = await _db.UserGroupMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .ToListAsync();

        // 3. Fetch all current CourseId assignments for these departments
        var activeCourseIdsForMyDepartments = await _db.DepartmentCourseGroups
            .Where(dcg => myDepartmentIds.Contains(dcg.DepartmentId))
            .Join(_db.CourseGroupCourses.Where(cgc => !cgc.IsDeleted), 
                  dcg => dcg.CourseGroupId, 
                  cgc => cgc.GroupId, 
                  (dcg, cgc) => cgc.CourseId)
            .Distinct()
            .ToListAsync();

        // 4. Calculate Level 1 Completion Status
        bool isLevel1Completed = true;
        if (level1CourseIds.Any())
        {
            var completedCount = await _db.Enrollments
                .CountAsync(enroll => enroll.UserId == userId && level1CourseIds.Contains(enroll.CourseId) && enroll.Status == "Completed");
            isLevel1Completed = completedCount >= level1CourseIds.Count;
        }

        var result = new List<EnrollmentResponse>();
        foreach (var e in enrollments)
        {
            // [MỚI]: Ẩn hoàn toàn khoá Cấp 2 nếu chưa xong Cấp 1
            if (e.Course.Level == 2 && !isLevel1Completed)
            {
                continue;
            }

            // [SYNC LOGIC]: If this was an auto-enrollment via a group/department
            if (e.GroupEnrollId.HasValue)
            {
                // Kiểm tra xem User còn ở trong phòng ban đó không VÀ Khoá học đó còn thuộc phòng ban đó không
                if (!myDepartmentIds.Contains(e.GroupEnrollId.Value) || !activeCourseIdsForMyDepartments.Contains(e.CourseId))
                {
                    // Skip if the association is no longer valid
                    continue;
                }
            }

            result.Add(await MapEnrollmentAsync(e));
        }
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

        // [MỚI] Tính toán trạng thái Khoá (Locked)
        bool isLocked = false;
        if (e.Course.Level == 2 && (e.AssignedById == null || e.GroupEnrollId != null))
        {
            var level1CourseIds = await _db.Courses
                .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
                .Select(c => c.Id)
                .ToListAsync();

            if (level1CourseIds.Any())
            {
                var completedLevel1Count = await _db.Enrollments
                    .CountAsync(enroll => enroll.UserId == e.UserId && level1CourseIds.Contains(enroll.CourseId) && enroll.Status == "Completed");
                
                if (completedLevel1Count < level1CourseIds.Count)
                {
                    isLocked = true;
                }
            }
        }

        return new EnrollmentResponse(
            e.Id, e.UserId, e.User.FullName, e.User.Avatar, e.CourseId, e.Course.Title,
            e.Status, e.Deadline, e.IsMandatory, e.EnrolledAt, Math.Round(progress, 1), isOverdue,
            totalLessons, completedLessons, isLocked, e.Course.Level, e.GroupEnrollId);
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