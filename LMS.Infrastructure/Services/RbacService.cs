using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class RbacService : IRbacService
{
    private readonly AppDbContext _db;
    public RbacService(AppDbContext db) => _db = db;

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        return await _db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Id)
            .Select(
                r => new RoleDto(
                    r.Id,
                    r.Name,
                    r.Description,
                    r.IsSystem,
                    r.RolePermissions.Select(rp => rp.Permission.Code).ToList()))
            .ToListAsync();
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        var role = new Role { Name = request.Name, Description = request.Description };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return new RoleDto(role.Id, role.Name, role.Description, false, new List<string>());
    }

    public async Task<RoleDto> UpdateRolePermissionsAsync(int roleId, AssignPermissionsRequest request)
    {
        var role = await _db.Roles
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId) ??
            throw new KeyNotFoundException("Không tìm thấy Role.");

        _db.RolePermissions.RemoveRange(role.RolePermissions);

        foreach(var permId in request.PermissionIds)
        {
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });
        }

        await _db.SaveChangesAsync();

        var updated = await _db.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstAsync(r => r.Id == roleId);

        return new RoleDto(
            updated.Id,
            updated.Name,
            updated.Description,
            updated.IsSystem,
            updated.RolePermissions.Select(rp => rp.Permission.Code).ToList());
    }

    public async Task DeleteRoleAsync(int roleId)
    {
        var role = await _db.Roles.FindAsync(roleId) ?? throw new KeyNotFoundException("Không tìm thấy Role.");
        if(role.IsSystem)
            throw new InvalidOperationException("Không thể xóa Role hệ thống.");
        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync()
    {
        return await _db.Permissions
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Code)
            .Select(p => new PermissionDto(p.Id, p.Code, p.Name, p.Category))
            .ToListAsync();
    }

    public async Task<UserRoleDto> GetUserRolesAsync(int userId)
    {
        var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId) ??
            throw new KeyNotFoundException("Không tìm thấy User.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        return new UserRoleDto(user.Id, user.FullName, user.Email, roles, user.Avatar);
    }

    public async Task<UserRoleDto> UpdateUserRolesAsync(int userId, AssignRolesRequest request)
    {
        var user = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId) ??
            throw new KeyNotFoundException("Không tìm thấy User.");

        _db.UserRoles.RemoveRange(user.UserRoles);

        foreach(var roleId in request.RoleIds)
        {
            _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
        }

        var firstRole = await _db.Roles.FindAsync(request.RoleIds.FirstOrDefault());
        if(firstRole != null)
            user.Role = firstRole.Name;

        await _db.SaveChangesAsync();

        var updated = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == userId);

        return new UserRoleDto(
            updated.Id,
            updated.FullName,
            updated.Email,
            updated.UserRoles.Select(ur => ur.Role.Name).ToList(),
            updated.Avatar);
    }
}
