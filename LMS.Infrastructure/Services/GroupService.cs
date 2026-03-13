using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class GroupService : IGroupService
{
    private readonly AppDbContext _db;

    public GroupService(AppDbContext db)
    {
        _db = db;
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

        return new UserGroupDetailResponse(group.Id, group.Name, group.Description, members, group.CreatedAt, group.UpdatedAt, group.IsDeleted);
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
                        GroupEnrollId = groupId,
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
                        GroupEnrollId = departmentId,
                        EnrolledAt = DateTime.UtcNow,
                        ApprovedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Cuối cùng mới Save
        await _db.SaveChangesAsync();
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
                g.CreatedAt))
            .ToListAsync();

        return new PagedResponse<CourseGroupResponse>(items, total, page, pageSize);
    }

    public async Task<CourseGroupDetailResponse?> GetCourseGroupDetailAsync(int groupId)
    {
        var group = await _db.CourseGroups
            .Include(g => g.Courses)
                .ThenInclude(c => c.Course)
                    .ThenInclude(c => c.Category) // Lấy tên Category để gán vào CourseResponse
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null) return null;

        var courses = group.Courses
            .OrderBy(c => c.SortOrder)
            .Select(c => new CourseResponse(
                c.Course.Id, c.Course.Title, c.Course.Description, c.Course.CoverImageUrl,
                c.Course.IsPublished, c.Course.PassScore, "Unknown", c.Course.CreatedAt,
                0, 0, 0, // Simplified counts
                c.Course.EnrollStartDate, c.Course.EnrollEndDate,
                c.Course.CompletionDeadlineDays, c.Course.CompletionEndDate,
                c.Course.RequiresApproval, null,
                c.Course.CategoryId,
                c.Course.Category != null ? c.Course.Category.Name : null)) // Đã thêm đủ 2 trường Category
            .ToList();

        return new CourseGroupDetailResponse(group.Id, group.Name, group.Description, courses, group.CreatedAt);
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

        return new CourseGroupResponse(group.Id, group.Name, group.Description, 0, 0, group.CreatedAt);
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

        return new CourseGroupResponse(group.Id, group.Name, group.Description, group.Courses.Count, departmentCount, group.CreatedAt);
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
}