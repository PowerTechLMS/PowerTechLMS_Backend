using System.Security.Claims;
using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(IUserService userService, IWebHostEnvironment environment)
    {
        _userService = userService;
        _environment = environment;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            var user = await _userService.GetUserProfileAsync(UserId);
            Console.WriteLine($"[DEBUG] GetProfile for User ID: {UserId}. Avatar: {user.Avatar}");
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Lỗi xác thực người dùng!" });
        }
    }

    [HttpPut]
    public async Task<ActionResult<UserResponse>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var updatedUser = await _userService.UpdateProfileAsync(UserId, request);
            return Ok(updatedUser);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPut("password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            await _userService.ChangePasswordAsync(UserId, request);
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("avatar")]
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            Console.WriteLine($"[DEBUG] Uploading avatar for User ID: {UserId}. File: {file.FileName}, Length: {file.Length}");
            
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadsPath))
            {
                Console.WriteLine($"[DEBUG] Creating directory: {uploadsPath}");
                Directory.CreateDirectory(uploadsPath);
            }

            // Sanitize filename: remove special characters and spaces
            var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
            safeFileName = System.Text.RegularExpressions.Regex.Replace(safeFileName, @"[^a-zA-Z0-9_-]", "");
            if (string.IsNullOrEmpty(safeFileName)) safeFileName = "avatar";
            
            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            Console.WriteLine($"[DEBUG] Saving file to: {filePath}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/avatars/{fileName}";
            Console.WriteLine($"[DEBUG] Updating database with URL: {avatarUrl}");
            await _userService.UpdateAvatarAsync(UserId, avatarUrl);
            
            return Ok(new { avatar = avatarUrl });
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }
}
