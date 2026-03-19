using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly IProgressService _progressService;

    public GroupService(AppDbContext db, INotificationService notificationService, IProgressService progressService)
    {
        _db = db;
        _notificationService = notificationService;
        _progressService = progressService;
    }

    // ==========================================
    // User Groups
    // ==========================================

    public async Task<PagedResponse<UserGroupResponse>> GetUserGroupsAsync(int page, int pageSize, string? search)
    {
        // Đã thay đổi DTO để trả về thêm số lượng Khung đào tạo (Course Groups)
        var query = _db.UserGroups.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.Name.Contains(search) || (g.Description != null && g.Description.Contains(search)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new UserGroupResponse(
                g.Id,
                g.Name,
                g.Description,
                _db.UserGroupMembers.Count(m => m.GroupId == g.Id), // Đếm nhân sự
                _db.DepartmentCourseGroups.Count(dcg => dcg.DepartmentId == g.Id), // MỚI: Đếm khung đào tạo áp dụng
                g.CreatedAt))
            .ToListAsync();

        return new PagedResponse<UserGroupResponse>(items, total, page, pageSize);
    }

    public async Task<UserGroupDetailResponse?> GetUserGroupDetailAsync(int groupId)
    {
        var group = await _db.UserGroups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return null;

        var members = group.Members
            .Select(m => new UserGroupMemberResponse(
                m.User.Id,
                m.User.FullName,
                m.User.Email,
                m.User.Role,
                m.User.IsActive,
                m.User.Avatar,
                m.AddedAt))
            .ToList();

        var courseGroupIds = await _db.DepartmentCourseGroups
            .Where(dcg => dcg.DepartmentId == groupId)
            .Select(dcg => dcg.CourseGroupId)
            .ToListAsync();

        return new UserGroupDetailResponse(group.Id, group.Name, group.Description, members, courseGroupIds, group.CreatedAt, group.UpdatedAt, group.IsDeleted);
    }

    public async Task<UserGroupResponse> CreateUserGroupAsync(UserGroupRequest request, int adminId)
    {
        var group = new UserGroup
        {
            Name = request.Name,
            Description = request.Description,
            CreatedById = adminId
        };

        _db.UserGroups.Add(group);
        await _db.SaveChangesAsync();

        // SỬA DÒNG NÀY: Truyền thêm số 0 cho CourseGroupCount
        return new UserGroupResponse(group.Id, group.Name, group.Description, 0, 0, group.CreatedAt);
    }

    public async Task<UserGroupResponse> UpdateUserGroupAsync(int groupId, UserGroupRequest request)
    {
        var group = await _db.UserGroups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("User group not found");

        group.Name = request.Name;
        group.Description = request.Description;
        await _db.SaveChangesAsync();

        // SỬA TẠI ĐÂY: Lấy số lượng khung đào tạo và truyền vào
        var courseGroupCount = await _db.DepartmentCourseGroups.CountAsync(d => d.DepartmentId == groupId);

        return new UserGroupResponse(group.Id, group.Name, group.Description, group.Members.Count, courseGroupCount, group.CreatedAt);
    }

    public async Task DeleteUserGroupAsync(int groupId)
    {
        var group = await _db.UserGroups.FindAsync(groupId) ?? throw new KeyNotFoundException("User group not found");
        _db.UserGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task AddUserToGroupAsync(int groupId, int userId, int addedById)
    {
        var exists = await _db.UserGroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId);
        if (exists) throw new InvalidOperationException("User is already in this group");

        _db.UserGroupMembers.Add(new UserGroupMember
        {
            GroupId = groupId,
            UserId = userId,
            AddedById = addedById,
            AddedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        // --- BỔ SUNG LOGIC: TỰ ĐỘNG GHI DANH KHI VÀO PHÒNG ---

        // Lấy tất cả CourseGroupId mà Phòng ban này đang áp dụng
        var activeCourseGroupIds = await _db.DepartmentCourseGroups
            .Where(dcg => dcg.DepartmentId == groupId)
            .Select(dcg => dcg.CourseGroupId)
            .ToListAsync();

        if (activeCourseGroupIds.Any())
        {
            // Lấy tất cả CourseId thuộc các CourseGroup đó
            var courseIds = await _db.CourseGroupCourses
                .Where(cgc => activeCourseGroupIds.Contains(cgc.GroupId) && !cgc.IsDeleted)
                .Select(cgc => cgc.CourseId)
                .Distinct()
                .ToListAsync();

            foreach (var courseId in courseIds)
            {
                var isEnrolled = await _db.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && !e.IsDeleted);

                if (!isEnrolled)
                {
                    _db.Enrollments.Add(new Enrollment
                    {
                        UserId = userId,
                        CourseId = courseId,
                        Status = "Approved",
                        IsMandatory = true,
                        AssignedById = addedById,
                        GroupEnrollId = groupId, // Gán Id của UserGroup này vào
                        EnrolledAt = DateTime.UtcNow,
                        ApprovedAt = DateTime.UtcNow
                    });
                }
            }
            await _db.SaveChangesAsync();
        }
    }

    public async Task RemoveUserFromGroupAsync(int groupId, int userId)
    {
        var member = await _db.UserGroupMembers.FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId)
            ?? throw new KeyNotFoundException("User is not in group");

        _db.UserGroupMembers.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task AssignCourseGroupToDepartmentAsync(int departmentId, int courseGroupId, int adminId)
    {
        // 1. Kiểm tra xem đã gán Group chưa. Nếu đã gán rồi thì KHÔNG quăng lỗi, chỉ return để Frontend chạy tiếp trơn tru.
        var exists = await _db.DepartmentCourseGroups
            .AnyAsync(d => d.DepartmentId == departmentId && d.CourseGroupId == courseGroupId);

        if (!exists)
        {
            var mapping = new DepartmentCourseGroup
            {
                DepartmentId = departmentId,
                CourseGroupId = courseGroupId,
                AssignedById = adminId,
                AssignedAt = DateTime.UtcNow
            };
            _db.DepartmentCourseGroups.Add(mapping);
            await _db.SaveChangesAsync();
        }

        // 2. TỰ ĐỘNG GHI DANH (AUTO-ENROLLMENT) - Xử lý chống trùng lặp cực mạnh
        var userIds = await _db.UserGroupMembers
            .Where(ugm => ugm.GroupId == departmentId && !ugm.IsDeleted)
            .Select(ugm => ugm.UserId)
            .ToListAsync();

        var courseIds = await _db.CourseGroupCourses
            .Where(cgc => cgc.GroupId == courseGroupId && !cgc.IsDeleted)
            .Select(cgc => cgc.CourseId)
            .ToListAsync();

        // Lấy trước toàn bộ ghi danh hiện tại của những User này để so sánh trên RAM (Nhanh & Không lỗi SQL)
        var existingEnrollments = await _db.Enrollments
            .Where(e => userIds.Contains(e.UserId) && courseIds.Contains(e.CourseId) && !e.IsDeleted)
            .Select(e => new { e.UserId, e.CourseId })
            .ToListAsync();

        foreach (var userId in userIds)
        {
            foreach (var courseId in courseIds)
            {
                // Chỉ thêm mới nếu tổ hợp UserId + CourseId chưa tồn tại
                if (!existingEnrollments.Any(e => e.UserId == userId && e.CourseId == courseId))
                {
                    _db.Enrollments.Add(new Enrollment
                    {
                        UserId = userId,
                        CourseId = courseId,
                        Status = "Approved",
                        IsMandatory = true,
                        AssignedById = adminId,
                        GroupEnrollId = departmentId, // Đây là Id của UserGroup (Phòng ban)
                        EnrolledAt = DateTime.UtcNow,
                        ApprovedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Cuối cùng mới Save
        await _db.SaveChangesAsync();

        // [NOTIFICATION] Notify all users in the department
        var courseGroup = await _db.CourseGroups.FindAsync(courseGroupId);
        foreach (var userId in userIds)
        {
            await _notificationService.CreateNotificationAsync(
                userId,
                "Lộ trình học tập mới",
                $"Phòng ban của bạn vừa được gán lộ trình học tập: {courseGroup?.Name ?? "Lộ trình mới"}",
                $"/learning-paths",
                "NewLearningPath"
            );
        }
    }

    public async Task RemoveCourseGroupFromDepartmentAsync(int departmentId, int courseGroupId)
    {
        var mapping = await _db.DepartmentCourseGroups
            .FirstOrDefaultAsync(d => d.DepartmentId == departmentId && d.CourseGroupId == courseGroupId);

        if (mapping != null)
        {
            _db.DepartmentCourseGroups.Remove(mapping);

            // Tùy chọn: Xóa ghi danh của các thành viên (nhưng user nói "Các khóa học hiện tại sẽ không bị xóa" khi gỡ thành viên, 
            // có lẽ không nên xóa ghi danh của toàn phòng ban khi gỡ lộ trình để tránh mất tiến độ?)
            // Ở đây ta chỉ gỡ mapping giữa Phòng ban và Lộ trình.

            await _db.SaveChangesAsync();
        }
    }
    // ==========================================
    // Course Groups
    // ==========================================

    public async Task<PagedResponse<CourseGroupResponse>> GetCourseGroupsAsync(int page, int pageSize, string? search)
    {
        var query = _db.CourseGroups.Include(g => g.Courses).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(g => g.Name.Contains(search) || (g.Description != null && g.Description.Contains(search)));

        var total = await query.CountAsync();

        // ĐIỀU CHỈNH: Lấy thêm số lượng Phòng ban đang bị áp Lộ trình này
        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new CourseGroupResponse(
                g.Id,
                g.Name,
                g.Description,
                g.Courses.Count, // Số lượng khóa học trong lộ trình
                _db.DepartmentCourseGroups.Count(dcg => dcg.CourseGroupId == g.Id), // BỔ SUNG: Số lượng phòng ban áp dụng
                g.CreatedAt,
                0, 0, 0)) // Thêm default values
            .ToListAsync();

        return new PagedResponse<CourseGroupResponse>(items, total, page, pageSize);
    }

    public async Task<CourseGroupDetailResponse?> GetCourseGroupDetailAsync(int groupId, int? userId = null)
    {
        var group = await _db.CourseGroups
            .Include(g => g.Courses)
                .ThenInclude(c => c.Course)
                    .ThenInclude(c => c.Category) // Lấy tên Category để gán vào CourseResponse
            .Include(g => g.Courses)
                .ThenInclude(c => c.Course)
                    .ThenInclude(c => c.Modules)
                        .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return null;

        var courses = group.Courses
            .OrderBy(c => c.SortOrder)
            .Select(c => new CourseResponse(
                c.Course.Id, c.Course.Title, c.Course.Description, c.Course.CoverImageUrl,
                c.Course.IsPublished, c.Course.PassScore, "Unknown", c.Course.CreatedAt,
                c.Course.Modules.Count,
                c.Course.Modules.Sum(m => m.Lessons.Count),
                0, // Enrollment count (simplified)
                c.Course.EnrollStartDate, c.Course.EnrollEndDate,
                c.Course.CompletionDeadlineDays, c.Course.CompletionEndDate,
                c.Course.RequiresApproval, null,
                c.Course.CategoryId,
                c.Course.Category != null ? c.Course.Category.Name : null,
                c.Course.Level,
                c.Course.QuizRetakeWaitTimeMinutes,
                c.Course.QuizMaxRetakesPerDay)) 
            .ToList();

        double progress = 0;
        int totalHours = 0;

        var courseIds = courses.Select(c => c.Id).ToList();

        if (userId.HasValue && courseIds.Any())
        {
            int totalItemsDetail = 0;
            int completedItemsDetail = 0;
            foreach (var cId in courseIds)
            {
                var p = await _progressService.GetCourseProgressAsync(userId.Value, cId);
                if (p != null)
                {
                    totalItemsDetail += p.TotalLessons + p.TotalQuizzes;
                    completedItemsDetail += p.CompletedLessons + p.PassedQuizzes;
                }
            }
            progress = totalItemsDetail > 0 ? (double)completedItemsDetail / totalItemsDetail * 100 : 0;
        }

        // Tính tổng thời lượng của tất cả khoá học trong lộ trình
        var totalSeconds = await _db.Lessons
            .Where(l => _db.Modules
                .Where(m => courseIds.Contains(m.CourseId))
                .Select(m => m.Id)
                .Contains(l.ModuleId))
            .SumAsync(l => l.VideoDurationSeconds + l.ReadingDurationSeconds);
        totalHours = (int)Math.Ceiling(totalSeconds / 3600.0);

        return new CourseGroupDetailResponse(group.Id, group.Name, group.Description, courses, group.CreatedAt, Math.Round(progress, 1), totalHours);
    }

    public async Task<CourseGroupResponse> CreateCourseGroupAsync(CourseGroupRequest request, int adminId)
    {
        var group = new CourseGroup
        {
            Name = request.Name,
            Description = request.Description,
            CreatedById = adminId
        };

        _db.CourseGroups.Add(group);
        await _db.SaveChangesAsync();

        return new CourseGroupResponse(group.Id, group.Name, group.Description, 0, 0, group.CreatedAt, 0, 0, 0);
    }

    public async Task<CourseGroupResponse> UpdateCourseGroupAsync(int groupId, CourseGroupRequest request)
    {
        var group = await _db.CourseGroups.Include(g => g.Courses).FirstOrDefaultAsync(g => g.Id == groupId)
            ?? throw new KeyNotFoundException("Course group not found");

        group.Name = request.Name;
        group.Description = request.Description;
        await _db.SaveChangesAsync();

        // SỬA TẠI ĐÂY: Đếm số phòng ban đang dùng và truyền vào
        var departmentCount = await _db.DepartmentCourseGroups.CountAsync(d => d.CourseGroupId == groupId);

        return new CourseGroupResponse(group.Id, group.Name, group.Description, group.Courses.Count, departmentCount, group.CreatedAt, 0, 0, 0);
    }

    public async Task DeleteCourseGroupAsync(int groupId)
    {
        var group = await _db.CourseGroups.FindAsync(groupId) ?? throw new KeyNotFoundException("Course group not found");
        _db.CourseGroups.Remove(group);
        await _db.SaveChangesAsync();
    }

    public async Task AddCourseToGroupAsync(int groupId, int courseId, int? sortOrder = null)
    {
        var exists = await _db.CourseGroupCourses.AnyAsync(m => m.GroupId == groupId && m.CourseId == courseId);
        if (exists) throw new InvalidOperationException("Course already in group");

        var maxOrder = await _db.CourseGroupCourses
            .Where(c => c.GroupId == groupId)
            .MaxAsync(c => (int?)c.SortOrder) ?? 0;

        _db.CourseGroupCourses.Add(new CourseGroupCourse
        {
            GroupId = groupId,
            CourseId = courseId,
            SortOrder = sortOrder ?? (maxOrder + 1)
        });
        await _db.SaveChangesAsync();
    }

    public async Task RemoveCourseFromGroupAsync(int groupId, int courseId)
    {
        var member = await _db.CourseGroupCourses.FirstOrDefaultAsync(m => m.GroupId == groupId && m.CourseId == courseId)
            ?? throw new KeyNotFoundException("Course is not in group");

        _db.CourseGroupCourses.Remove(member);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CourseGroupResponse>> GetMyCourseGroupsAsync(int userId)
    {
        // 1. Tìm các Phòng ban (UserGroups) mà User này tham gia
        var userGroupIds = await _db.UserGroupMembers
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Select(m => m.GroupId)
            .ToListAsync();

        if (!userGroupIds.Any()) return new List<CourseGroupResponse>();

        // 2. Tìm các Lộ trình (CourseGroups) được gán cho các Phòng ban đó
        var courseGroupIds = await _db.DepartmentCourseGroups
            .Where(dcg => userGroupIds.Contains(dcg.DepartmentId))
            .Select(dcg => dcg.CourseGroupId)
            .Distinct()
            .ToListAsync();

        if (!courseGroupIds.Any()) return new List<CourseGroupResponse>();

        // 3. Trả về thông tin Lộ trình kèm tiến độ thực tế & tổng thời lượng
        var groups = await _db.CourseGroups
            .Where(cg => courseGroupIds.Contains(cg.Id))
            .ToListAsync();

        var result = new List<CourseGroupResponse>();
        foreach (var cg in groups)
        {
            var courseGroupCourses = await _db.CourseGroupCourses
                .Where(cgc => cgc.GroupId == cg.Id && !cgc.IsDeleted)
                .Select(cgc => cgc.CourseId)
                .ToListAsync();

            if (!courseGroupCourses.Any())
            {
                result.Add(new CourseGroupResponse(cg.Id, cg.Name, cg.Description, 0, 0, cg.CreatedAt, 0, 0, 0));
                continue;
            }

            // Phải lấy danh sách enrollment để phục vụ việc kiểm tra chứng chỉ
            var enrollments = await _db.Enrollments
                .Where(e => e.UserId == userId && courseGroupCourses.Contains(e.CourseId) && !e.IsDeleted)
                .ToListAsync();

            // Lấy tiến độ thực tế theo tỷ lệ (bài học + bài thi) đã xong / tổng toàn bộ bài học + bài thi của mọi khóa trong lộ trình
            int totalItemsInPath = 0;
            int completedItemsInPath = 0;

            foreach (var cId in courseGroupCourses)
            {
                var p = await _progressService.GetCourseProgressAsync(userId, cId);
                if (p != null)
                {
                    totalItemsInPath += p.TotalLessons + p.TotalQuizzes;
                    completedItemsInPath += p.CompletedLessons + p.PassedQuizzes;
                }
            }

            double avgProgress = totalItemsInPath > 0 ? (double)completedItemsInPath / totalItemsInPath * 100 : 0;

            // Đếm chứng chỉ chờ (Số khoá ĐÃ HOÀN THÀNH nhưng CHƯA CÓ CERTIFICATE cho User này)
            var completedCourseIds = enrollments.Where(e => e.Status == "Completed" || e.Status == "Finished").Select(e => e.CourseId).ToList();
            var certedCourseIds = await _db.Certificates.Where(c => c.UserId == userId && completedCourseIds.Contains(c.CourseId)).Select(c => c.CourseId).ToListAsync();
            int pendingCerts = completedCourseIds.Count - certedCourseIds.Count;

            // Tính tổng thời lượng của tất cả khoá học trong lộ trình
            var totalSeconds = await _db.Lessons
                .Where(l => _db.Modules
                    .Where(m => courseGroupCourses.Contains(m.CourseId))
                    .Select(m => m.Id)
                    .Contains(l.ModuleId))
                .SumAsync(l => l.VideoDurationSeconds + l.ReadingDurationSeconds);
            int totalHours = (int)Math.Ceiling(totalSeconds / 3600.0);

            result.Add(new CourseGroupResponse(
                cg.Id,
                cg.Name,
                cg.Description,
                courseGroupCourses.Count,
                await _db.DepartmentCourseGroups.CountAsync(dcg => dcg.CourseGroupId == cg.Id),
                cg.CreatedAt,
                Math.Round(avgProgress, 1),
                pendingCerts,
                totalHours
            ));
        }

        return result;
    }
}