using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LMS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public AuthService(AppDbContext db, IConfiguration config, IEmailService emailService)
    {
        _db = db;
        _config = config;
        _emailService = emailService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if(user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        if(!user.IsActive)
            throw new UnauthorizedAccessException("Tài khoản của bạn đã bị vô hiệu hóa.");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var (roles, permissions) = await GetUserRolesAndPermissionsAsync(user.Id);

        if(!roles.Any())
            roles = new List<string> { user.Role };

        var token = GenerateToken(user, roles, permissions);
        return new AuthResponse(user.Id, user.FullName, user.Email, user.Role, token, roles, permissions, user.Avatar);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if(await _db.Users.AnyAsync(u => u.Email == request.Email))
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

        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == (request.Role ?? "Employee"));
        if(defaultRole != null)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = defaultRole.Id });
            await _db.SaveChangesAsync();
        }

        _emailService.QueueEmail(
            user.Email,
            "Chào mừng bạn gia nhập hệ thống!",
            $"Chào mừng {user.FullName}! Tài khoản của bạn đã được khởi tạo.");

        var (roles, permissions) = await GetUserRolesAndPermissionsAsync(user.Id);
        if(!roles.Any())
            roles = new List<string> { user.Role };

        var token = GenerateToken(user, roles, permissions);
        return new AuthResponse(user.Id, user.FullName, user.Email, user.Role, token, roles, permissions, user.Avatar);
    }
    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null) return; 

        var otp = new Random().Next(100000, 999999).ToString();
        user.ResetPasswordOtp = otp;
        user.OtpExpiry = DateTime.UtcNow.AddMinutes(10);
        await _db.SaveChangesAsync();

        _emailService.QueueEmail(
            user.Email,
            "Mã OTP đặt lại mật khẩu",
            $@"
            <p>Chào {user.FullName},</p>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản LMS.</p>
            <p>Mã OTP của bạn là: <strong style='font-size: 24px; color: #1e3a8a;'>{otp}</strong></p>
            <p>Mã này có hiệu lực trong vòng 10 phút. Vui lòng không chia sẻ mã này cho bất kỳ ai.</p>
            <br/>
            <p>Nếu bạn không yêu cầu điều này, vui lòng bỏ qua email này.</p>");
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || user.ResetPasswordOtp != request.Otp || user.OtpExpiry < DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || user.ResetPasswordOtp != request.Otp || user.OtpExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Mã OTP không hợp lệ hoặc đã hết hạn.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPasswordOtp = null;
        user.OtpExpiry = null;
        await _db.SaveChangesAsync();
    }

    /// <summary>
    /// Truy vấn bảng RBAC để lấy danh sách roles và permissions của user. UserRoles → Role → RolePermissions →
    /// Permission
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

        foreach(var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        foreach(var perm in permissions)
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
