using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace LMS.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public UserService(AppDbContext db, IAuthService authService, IEmailService emailService)
    {
        _db = db;
        _authService = authService;
        _emailService = emailService;
    }

    public async Task<PagedResponse<UserResponse>> GetUsersAsync(int page, int pageSize, string? search)
    {
        var query = _db.Users.AsQueryable();
        if(!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(
                u => new UserResponse(
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.Phone,
                    u.Address,
                    u.Bio,
                    u.Avatar,
                    u.CreatedAt,
                    _db.UserGroupMembers.Where(gm => gm.UserId == u.Id).Select(gm => gm.Group.Name).FirstOrDefault(),
                    _db.UserGroupMembers.Where(gm => gm.UserId == u.Id).Select(gm => (int?)gm.GroupId).FirstOrDefault(),
                    u.Position))
            .ToListAsync();

        return new PagedResponse<UserResponse>(items, total, page, pageSize);
    }

    public async Task ToggleActiveAsync(int userId, int adminId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        if(user.Id == adminId)
            throw new InvalidOperationException("Không thể tự khóa tài khoản của chính mình.");

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<UserResponse> GetUserProfileAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Tài khoản không tồn tại.");

        var group = await _db.UserGroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => new { Name = gm.Group.Name, Id = (int?)gm.GroupId })
            .FirstOrDefaultAsync();

        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.IsActive,
            user.Phone,
            user.Address,
            user.Bio,
            user.Avatar,
            user.CreatedAt,
            group?.Name,
            group?.Id,
            user.Position);
    }

    public async Task<UserProfileReportResponse> GetUserProfileReportAsync(int userId)
    {
        var enrollments = await _db.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var certificates = await _db.Certificates.CountAsync(c => c.UserId == userId && c.Status == "Valid");
        var lessonProgresses = await _db.LessonProgresses.Where(lp => lp.UserId == userId).ToListAsync();

        var coursesList = new List<UserCourseProgressDto>();
        int completedCourses = 0;

        foreach(var e in enrollments)
        {
            var totalLessons = e.Course.Modules.SelectMany(m => m.Lessons).Count();
            var completedLessons = e.Course.Modules
                .SelectMany(m => m.Lessons)
                .Count(l => lessonProgresses.Any(lp => lp.LessonId == l.Id && lp.IsCompleted));

            double progress = totalLessons == 0 ? 0 : Math.Round((double)completedLessons / totalLessons * 100, 1);
            if(progress >= 100)
                completedCourses++;

            var lastAccess = lessonProgresses
                .Where(lp => e.Course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).Contains(lp.LessonId))
                .Max(lp => (DateTime?)lp.UpdatedAt);

            coursesList.Add(
                new UserCourseProgressDto(
                    e.CourseId,
                    e.Course.Title,
                    progress,
                    progress >= 100 ? "Completed" : "Learning",
                    lastAccess ?? e.EnrolledAt));
        }

        return new UserProfileReportResponse(enrollments.Count, completedCourses, certificates, coursesList);
    }

    public async Task<UserResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Tài khoản không tồn tại.");

        user.FullName = request.FullName;
        user.Phone = request.Phone;
        user.Address = request.Address;
        user.Bio = request.Bio;

        if(!string.IsNullOrEmpty(request.Avatar))
        {
            user.Avatar = request.Avatar;
        }

        await _db.SaveChangesAsync();
        return await GetUserProfileAsync(userId);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Tài khoản không tồn tại.");

        if(!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Mật khẩu hiện tại không chính xác.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAvatarAsync(int userId, string avatarUrl)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Tài khoản không tồn tại.");
        user.Avatar = avatarUrl;
        await _db.SaveChangesAsync();
    }

    public async Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.Role = request.Role;
        user.Position = request.Position;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        
        // Cập nhật RBAC
        var existingRole = user.UserRoles.FirstOrDefault();
        if(existingRole == null || existingRole.Role.Name != request.Role)
        {
            if(existingRole != null) _db.UserRoles.Remove(existingRole);
            
            var newRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
            if(newRole != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = newRole.Id });
            }
        }

        if(!string.IsNullOrWhiteSpace(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        if(request.GroupId.HasValue)
        {
            var existingMember = await _db.UserGroupMembers.FirstOrDefaultAsync(m => m.UserId == userId);
            if(existingMember == null || existingMember.GroupId != request.GroupId.Value)
            {
                if(existingMember != null)
                    _db.UserGroupMembers.Remove(existingMember);

                _db.UserGroupMembers
                    .Add(
                        new UserGroupMember
                        {
                            UserId = userId,
                            GroupId = request.GroupId.Value,
                            AddedAt = DateTime.UtcNow
                        });

                var activeCourseGroupIds = await _db.DepartmentCourseGroups
                    .Where(dcg => dcg.DepartmentId == request.GroupId.Value)
                    .Select(dcg => dcg.CourseGroupId)
                    .ToListAsync();

                if(activeCourseGroupIds.Any())
                {
                    var courseIds = await _db.CourseGroupCourses
                        .Where(cgc => activeCourseGroupIds.Contains(cgc.GroupId) && !cgc.IsDeleted)
                        .Select(cgc => cgc.CourseId)
                        .Distinct()
                        .ToListAsync();

                    var existingCourseIds = await _db.Enrollments
                        .Where(e => e.UserId == userId && !e.IsDeleted)
                        .Select(e => e.CourseId)
                        .ToListAsync();

                    foreach(var courseId in courseIds)
                    {
                        if(!existingCourseIds.Contains(courseId))
                        {
                            _db.Enrollments
                                .Add(
                                    new Enrollment
                                    {
                                        UserId = userId,
                                        CourseId = courseId,
                                        Status = "Approved",
                                        IsMandatory = true,
                                        GroupEnrollId = request.GroupId.Value,
                                        EnrolledAt = DateTime.UtcNow,
                                        ApprovedAt = DateTime.UtcNow
                                    });
                        }
                    }
                }
            }
        } else
        {
            var existingMember = await _db.UserGroupMembers.FirstOrDefaultAsync(m => m.UserId == userId);
            if(existingMember != null)
                _db.UserGroupMembers.Remove(existingMember);
        }

        await _db.SaveChangesAsync();
        return await GetUserProfileAsync(userId);
    }

    public async Task<UserResponse> CreateUserAsync(UpdateUserRequest request)
    {
        if(await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã tồn tại.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? "123456"),
            Role = request.Role,
            Position = request.Position,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role);
        if(role != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        }

        if(request.GroupId.HasValue)
        {
            _db.UserGroupMembers
                .Add(
                    new UserGroupMember { UserId = user.Id, GroupId = request.GroupId.Value, AddedAt = DateTime.UtcNow });

            var activeCourseGroupIds = await _db.DepartmentCourseGroups
                .Where(dcg => dcg.DepartmentId == request.GroupId.Value)
                .Select(dcg => dcg.CourseGroupId)
                .ToListAsync();

            if(activeCourseGroupIds.Any())
            {
                var courseIds = await _db.CourseGroupCourses
                    .Where(cgc => activeCourseGroupIds.Contains(cgc.GroupId) && !cgc.IsDeleted)
                    .Select(cgc => cgc.CourseId)
                    .Distinct()
                    .ToListAsync();

                foreach(var courseId in courseIds)
                {
                    _db.Enrollments
                        .Add(
                            new Enrollment
                            {
                                UserId = user.Id,
                                CourseId = courseId,
                                Status = "Approved",
                                IsMandatory = true,
                                GroupEnrollId = request.GroupId.Value,
                                EnrolledAt = DateTime.UtcNow,
                                ApprovedAt = DateTime.UtcNow
                            });
                }
            }
        }

        await _db.SaveChangesAsync();

        _emailService.QueueEmail(
            user.Email,
            "Chào mừng bạn đến với hệ thống!",
            $"Xin chào {user.FullName}, tài khoản của bạn đã được khởi tạo bởi Admin.<br/>Email: {user.Email}<br/>Mật khẩu: {request.Password ?? "123456"}");

        return await GetUserProfileAsync(user.Id);
    }

    public async Task UpdatePasswordAsync(int userId, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("Không tìm thấy user");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        await _db.SaveChangesAsync();
    }

    public async Task<ImportResultResponse> ImportUsersAsync(Stream fileStream)
    {
        var rows = MiniExcel.Query<UserImportRow>(fileStream).ToList();
        int successCount = 0;
        var errors = new List<string>();

        foreach(var row in rows)
        {
            if(string.IsNullOrWhiteSpace(row.Email) || string.IsNullOrWhiteSpace(row.FullName))
            {
                errors.Add($"Dòng lỗi: Thiếu Họ tên hoặc Email.");
                continue;
            }

            if(await _db.Users.AnyAsync(u => u.Email == row.Email))
            {
                errors.Add($"Email trùng lặp: {row.Email}");
                continue;
            }

            var password = !string.IsNullOrWhiteSpace(row.Password) ? row.Password : "123456";
            var role = !string.IsNullOrWhiteSpace(row.Role) ? row.Role : "Employee";

            var user = new User
            {
                FullName = row.FullName,
                Email = row.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var r = await _db.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if(r != null)
            {
                _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = r.Id });
            }
            successCount++;
        }

        await _db.SaveChangesAsync();
        return new ImportResultResponse(successCount, errors);
    }

    public async Task<object> SyncAllUserRolesAsync()
    {
        // Tải tất cả roles từ DB
        var allRoles = await _db.Roles.ToListAsync();
        var roleMap = allRoles.ToDictionary(r => r.Name, r => r.Id, StringComparer.OrdinalIgnoreCase);

        // Tải tất cả users kèm UserRoles hiện tại
        var users = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();

        int updated = 0;
        int skipped = 0;

        foreach(var user in users)
        {
            if(string.IsNullOrWhiteSpace(user.Role)) { skipped++; continue; }

            var currentRoleNames = user.UserRoles.Select(ur => ur.Role?.Name).ToList();
            bool alreadyCorrect = currentRoleNames.Any(r => 
                string.Equals(r, user.Role, StringComparison.OrdinalIgnoreCase));

            if(!alreadyCorrect)
            {
                // Xóa tất cả roles cũ
                _db.UserRoles.RemoveRange(user.UserRoles);

                // Gán role đúng
                if(roleMap.TryGetValue(user.Role, out var roleId))
                {
                    _db.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        AssignedAt = DateTime.UtcNow
                    });
                    updated++;
                }
            }
            else
            {
                skipped++;
            }
        }

        await _db.SaveChangesAsync();
        return new { message = $"Đồng bộ RBAC hoàn tất. Đã cập nhật: {updated}, Bỏ qua: {skipped}." };
    }
}
