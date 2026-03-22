using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                User.FindFirst("sub") ??
                User.FindFirst("id") ??
                User.FindFirst("UserId");
            if(claim == null)
                throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetProfile()
    {
        try
        {
            var user = await _userService.GetUserProfileAsync(UserId);
            return Ok(user);
        } catch(KeyNotFoundException)
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
        } catch(KeyNotFoundException ex)
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
        } catch(ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        } catch(KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("avatar")]
    public async Task<ActionResult> UploadAvatar(IFormFile file)
    {
        try
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
            if(!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
            safeFileName = Regex.Replace(safeFileName, @"[^a-zA-Z0-9_-]", string.Empty);
            if(string.IsNullOrEmpty(safeFileName))
                safeFileName = "avatar";

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}_{safeFileName}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using(var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var avatarUrl = $"/uploads/avatars/{fileName}";
            await _userService.UpdateAvatarAsync(UserId, avatarUrl);

            return Ok(new { avatar = avatarUrl });
        } catch(ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        } catch(KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
