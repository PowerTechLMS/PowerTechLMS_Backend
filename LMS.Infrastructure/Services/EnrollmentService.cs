using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public EnrollmentService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<EnrollmentResponse> EnrollAsync(int userId, int courseId)
    {
        var existing = await _db.Enrollments.FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        var course = await _db.Courses.FindAsync(courseId) ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");
        var user = await _db.Users.FindAsync(userId);
        bool isAdmin = user?.Role == "Admin";

        if(existing != null)
        {
            if(existing.Status == "Rejected" || existing.IsDeleted)
            {
                if(!isAdmin)
                    await ValidateEnrollmentInternalAsync(userId, courseId, course);

                existing.Status = course.RequiresApproval ? "Pending" : "Approved";
                existing.IsDeleted = false;
                existing.EnrolledAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return await MapEnrollmentAsync(existing);
            }
            throw new InvalidOperationException("Bạn đã ghi danh khóa học này.");
        }

        if(!isAdmin)
            await ValidateEnrollmentInternalAsync(userId, courseId, course);

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            Status = course.RequiresApproval ? "Pending" : "Approved"
        };
        _db.Enrollments.Add(enrollment);
        await _db.SaveChangesAsync();

        if(enrollment.Status == "Pending")
        {
            var admins = await _db.Users.Where(u => u.Role == "Admin").ToListAsync();
            foreach(var admin in admins)
            {
                await _notificationService.CreateNotificationAsync(
                    admin.Id,
                    "Yêu cầu phê duyệt ghi danh",
                    $"Người dùng {user?.FullName} yêu cầu tham gia khóa học: {course.Title}",
                    "/admin/enrollments",
                    "EnrollmentRequest");
            }
        }

        return await MapEnrollmentAsync(enrollment);
    }

    private async Task ValidateEnrollmentInternalAsync(int userId, int courseId, Course course)
    {
        var now = DateTime.UtcNow;
        if(course.EnrollStartDate != null && now < course.EnrollStartDate)
            throw new InvalidOperationException(
                $"Khóa học chưa mở đăng ký. Bắt đầu từ {course.EnrollStartDate:dd/MM/yyyy}.");
        if(course.EnrollEndDate != null && now > course.EnrollEndDate)
            throw new InvalidOperationException(
                $"Đã hết thời hạn đăng ký (hạn chót: {course.EnrollEndDate:dd/MM/yyyy}).");

        if(course.Level == 2)
        {
            var allowedCourseGroupIds = await _db.CourseGroupCourses
                .Where(cgc => cgc.CourseId == courseId && !cgc.IsDeleted)
                .Select(cgc => cgc.GroupId)
                .ToListAsync();

            var userDepartmentIds = await _db.UserGroupMembers
                .Where(ugm => ugm.UserId == userId && !ugm.IsDeleted)
                .Select(ugm => ugm.GroupId)
                .ToListAsync();

            var isDepartmentMatch = await _db.DepartmentCourseGroups
                .AnyAsync(
                    dcg => userDepartmentIds.Contains(dcg.DepartmentId) &&
                        allowedCourseGroupIds.Contains(dcg.CourseGroupId));

            if(!isDepartmentMatch)
                throw new InvalidOperationException("Khoá học chuyên ngành này không thuộc phòng ban của bạn.");

            var level1CourseIds = await _db.Courses
                .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
                .Select(c => c.Id)
                .ToListAsync();

            if(level1CourseIds.Any())
            {
                var completedLevel1Count = await _db.Enrollments
                    .CountAsync(
                        e => e.UserId == userId && level1CourseIds.Contains(e.CourseId) && e.Status == "Completed");

                if(completedLevel1Count < level1CourseIds.Count)
                    throw new InvalidOperationException(
                        "Bạn cần hoàn thành TOÀN BỘ các khóa học Bắt buộc (Cấp 1) trước khi đăng ký khóa học này.");
            }
        }

        if(course.Level == 3)
        {
            var level1And2CourseIds = await _db.Courses
                .Where(c => (c.Level == 1 || c.Level == 2) && !c.IsDeleted && c.IsPublished)
                .Select(c => c.Id)
                .ToListAsync();

            if(level1And2CourseIds.Any())
            {
                var completedCount = await _db.Enrollments
                    .CountAsync(
                        e => e.UserId == userId && level1And2CourseIds.Contains(e.CourseId) && e.Status == "Completed");

                if(completedCount < level1And2CourseIds.Count)
                    throw new InvalidOperationException(
                        "Bạn cần hoàn thành TOÀN BỘ các khóa học Cấp 1 & Cấp 2 trước khi đăng ký khóa học tự chọn này.");
            }
        }
    }

    public async Task<EnrollmentResponse> AdminEnrollAsync(AdminEnrollRequest request, int assignedById)
    {
        var existing = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == request.UserId && e.CourseId == request.CourseId);
        if(existing != null && !existing.IsDeleted)
            throw new InvalidOperationException("Nhân viên đã ghi danh khóa học này.");

        if(existing != null && existing.IsDeleted)
        {
            existing.IsDeleted = false;
            existing.Status = "Approved";
            existing.Deadline = request.Deadline;
            existing.IsMandatory = request.IsMandatory;
            existing.AssignedById = assignedById;
            existing.EnrolledAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return await MapEnrollmentAsync(existing);
        }

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

        await _notificationService.CreateNotificationAsync(
            request.UserId,
            "Bạn đã được ghi danh vào khóa học mới",
            $"Quản trị viên đã ghi danh bạn vào khóa học: {enrollment.Course?.Title ?? "Khóa học mới"}",
            $"/courses/{request.CourseId}",
            "EnrollmentStatus");

        return await MapEnrollmentAsync(enrollment);
    }

    public async Task<EnrollmentResponse> ApproveEnrollmentAsync(int enrollmentId, bool approved, string? reason = null)
    {
        var enrollment = await _db.Enrollments.FindAsync(enrollmentId) ??
            throw new KeyNotFoundException("Không tìm thấy ghi danh.");

        if(!approved && string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Phải cung cấp lý do khi từ chối.");

        enrollment.Status = approved ? "Approved" : "Rejected";
        if(!approved)
            enrollment.RejectionReason = reason;

        await _db.SaveChangesAsync();

        await _notificationService.CreateNotificationAsync(
            enrollment.UserId,
            approved ? "Yêu cầu ghi danh được chấp nhận" : "Yêu cầu ghi danh bị từ chối",
            approved
                ? $"Yêu cầu tham gia khóa học {enrollment.Course?.Title} của bạn đã được phê duyệt."
                : $"Yêu cầu tham gia khóa học {enrollment.Course?.Title} của bạn không được chấp nhận. Lý do: {reason}",
            approved ? $"/courses/{enrollment.CourseId}" : null,
            "EnrollmentStatus");

        return await MapEnrollmentAsync(enrollment);
    }

    public async Task<List<EnrollmentResponse>> GetUserEnrollmentsAsync(int userId)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(
                e => e.UserId == userId && (e.Status == "Approved" || e.Status == "Pending" || e.Status == "Completed"))
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var level1CourseIds = await _db.Courses
            .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
            .Select(c => c.Id)
            .ToListAsync();

        if(level1CourseIds.Any())
        {
            var myLevel1EnrollmentCourseIds = enrollments
                .Where(e => e.Course.Level == 1)
                .Select(e => e.CourseId)
                .ToList();

            var missingLevel1Ids = level1CourseIds.Except(myLevel1EnrollmentCourseIds).ToList();

            if(missingLevel1Ids.Any())
            {
                foreach(var courseId in missingLevel1Ids)
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

                    await _db.Entry(newEnroll).Reference(x => x.Course).LoadAsync();
                    enrollments.Add(newEnroll);
                }
                await _db.SaveChangesAsync();
            }
        }

        var myDepartmentIds = await _db.UserGroupMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.GroupId)
            .ToListAsync();

        var activeCourseIdsForMyDepartments = await _db.DepartmentCourseGroups
            .Where(dcg => myDepartmentIds.Contains(dcg.DepartmentId))
            .Join(
                _db.CourseGroupCourses.Where(cgc => !cgc.IsDeleted),
                dcg => dcg.CourseGroupId,
                cgc => cgc.GroupId,
                (dcg, cgc) => cgc.CourseId)
            .Distinct()
            .ToListAsync();

        bool isLevel1Completed = true;
        if(level1CourseIds.Any())
        {
            var completedCount = await _db.Enrollments
                .CountAsync(
                    enroll => enroll.UserId == userId &&
                        level1CourseIds.Contains(enroll.CourseId) &&
                        enroll.Status == "Completed");
            isLevel1Completed = completedCount >= level1CourseIds.Count;
        }

        var result = new List<EnrollmentResponse>();
        foreach(var e in enrollments)
        {
            if(e.GroupEnrollId.HasValue)
            {
                if(!myDepartmentIds.Contains(e.GroupEnrollId.Value) ||
                    !activeCourseIdsForMyDepartments.Contains(e.CourseId))
                {
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
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.CourseId == courseId)
            .OrderByDescending(e => e.EnrolledAt)
            .ToListAsync();

        var result = new List<EnrollmentResponse>();
        foreach(var e in enrollments)
            result.Add(await MapEnrollmentAsync(e));
        return result;
    }

    public async Task<List<EnrollmentResponse>> GetPendingEnrollmentsAsync(int userId, bool isAdmin = false)
    {
        var query = _db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.Status == "Pending")
            .AsQueryable();

        if(!isAdmin)
            query = query.Where(e => e.Course.CreatedById == userId);

        var enrollments = await query.OrderByDescending(e => e.EnrolledAt).ToListAsync();

        var result = new List<EnrollmentResponse>();
        foreach(var e in enrollments)
            result.Add(await MapEnrollmentAsync(e));
        return result;
    }

    private async Task<EnrollmentResponse> MapEnrollmentAsync(Enrollment e)
    {
        if(e.User == null)
            await _db.Entry(e).Reference(x => x.User).LoadAsync();
        if(e.Course == null)
            await _db.Entry(e).Reference(x => x.Course).LoadAsync();

        var lessonIds = await _db.Lessons
            .Where(l => l.Module.CourseId == e.CourseId && !l.IsDeleted)
            .Select(l => l.Id)
            .ToListAsync();

        int totalLessons = lessonIds.Count;
        int completedLessons = await _db.LessonProgresses
            .CountAsync(lp => lp.UserId == e.UserId && lp.IsCompleted && lessonIds.Contains(lp.LessonId));

        var progress = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;
        var isOverdue = e.Deadline.HasValue && e.Deadline.Value < DateTime.UtcNow && progress < 100;

        bool isLocked = false;
        if(e.Course?.Level == 2 && (e.AssignedById == null || e.GroupEnrollId != null))
        {
            var level1CourseIds = await _db.Courses
                .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
                .Select(c => c.Id)
                .ToListAsync();

            if(level1CourseIds.Any())
            {
                var completedLevel1Count = await _db.Enrollments
                    .CountAsync(
                        enroll => enroll.UserId == e.UserId &&
                            level1CourseIds.Contains(enroll.CourseId) &&
                            enroll.Status == "Completed");

                if(completedLevel1Count < level1CourseIds.Count)
                {
                    isLocked = true;
                }
            }
        }

        var deptName = await _db.UserGroupMembers
            .Where(m => m.UserId == e.UserId)
            .Select(m => m.Group.Name)
            .FirstOrDefaultAsync();

        var res = new EnrollmentResponse(
            e.Id,
            e.UserId,
            e.User?.FullName ?? "Học viên",
            e.User?.Email ?? "---",
            e.User?.Avatar,
            e.CourseId,
            e.Course?.Title ?? "Khóa học",
            e.Status,
            e.Deadline,
            e.IsMandatory,
            e.EnrolledAt,
            Math.Round(progress, 1),
            isOverdue,
            totalLessons,
            completedLessons,
            isLocked,
            e.Course?.Level ?? 3,
            e.GroupEnrollId,
            deptName,
            e.Course?.CoverImageUrl,
            e.RejectionReason,
            e.User?.Position);
        return res;
    }

    public async Task<object> GetAllEnrollmentsAsync(int page, int pageSize, int userId, bool isAdmin = false)
    {
        var query = _db.Enrollments.Include(e => e.Course).Include(e => e.User).Where(e => !e.IsDeleted).AsQueryable();

        if(!isAdmin)
            query = query.Where(e => e.Course.CreatedById == userId);

        var total = await query.CountAsync();

        var enrollments = await query
            .OrderByDescending(e => e.EnrolledAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<EnrollmentResponse>();

        foreach(var e in enrollments)
        {
            items.Add(await MapEnrollmentAsync(e));
        }

        return new { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }
}