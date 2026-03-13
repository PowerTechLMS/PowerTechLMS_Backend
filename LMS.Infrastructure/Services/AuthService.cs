using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị vô hiệu hóa.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var (roles, permissions) = await GetUserRolesAndPermissionsAsync(user.Id);

        // Fallback: nếu chưa có UserRole trong bảng RBAC, dùng legacy User.Role
        if (!roles.Any()) roles = new List<string> { user.Role };

        var token = GenerateToken(user, roles, permissions);
        return new AuthResponse(user.Id, user.FullName, user.Email, user.Role, token, roles, permissions, user.Avatar);

    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new InvalidOperationException("Email đã tồn tại.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role ?? "Employee"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Gán role mặc định Employee (RoleId = 3) cho user mới
        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == (request.Role ?? "Employee"));
        if (defaultRole != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
            await _db.SaveChangesAsync();
        }

        var (roles, permissions) = await GetUserRolesAndPermissionsAsync(user.Id);
        if (!roles.Any()) roles = new List<string> { user.Role };

        var token = GenerateToken(user, roles, permissions);
        return new AuthResponse(user.Id, user.FullName, user.Email, user.Role, token, roles, permissions, user.Avatar);
    }

    /// <summary>
    /// Truy vấn bảng RBAC để lấy danh sách roles và permissions của user.
    /// UserRoles → Role → RolePermissions → Permission
    /// </summary>
    private async Task<(List<string> roles, List<string> permissions)> GetUserRolesAndPermissionsAsync(int userId)
    {
        var userRoles = await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
            .ToListAsync();

        var roles = userRoles.Select(ur => ur.Role.Name).Distinct().ToList();

        var permissions = userRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Code)
            .Distinct()
            .ToList();

        return (roles, permissions);
    }

    private string GenerateToken(User user, List<string> roles, List<string> permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
        };

        // Thêm tất cả roles vào claims
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        // Thêm tất cả permissions vào claims
        foreach (var perm in permissions)
            claims.Add(new Claim("permission", perm));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
