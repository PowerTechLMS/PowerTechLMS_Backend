using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    private int AdminId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    [Authorize(Policy = "UserList")]
    public async Task<ActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null) => Ok(await _userService.GetUsersAsync(page, pageSize, search));

    [HttpPost]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult> CreateUser([FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.CreateUserAsync(request);
            return Ok(user);
        } catch(InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        } catch(Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "UserList")]
    public async Task<ActionResult> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserProfileAsync(id);
            return Ok(user);
        } catch(KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            await _userService.UpdateUserAsync(id, request);
            return Ok(new { message = "Cập nhật thành công!" });
        } catch(KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy người dùng." });
        } catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/report")]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult> GetUserReport(int id)
    {
        try
        {
            var report = await _userService.GetUserProfileReportAsync(id);
            return Ok(report);
        } catch(KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPut("{id}/toggle-active")]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult> ToggleActive(int id)
    {
        try
        {
            await _userService.ToggleActiveAsync(id, AdminId);
            return NoContent();
        } catch(KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("import")]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult> ImportUsers(IFormFile file)
    {
        if(file == null || file.Length == 0)
            return BadRequest("Vui lòng chọn file Excel.");
        using var stream = file.OpenReadStream();
        var result = await _userService.ImportUsersAsync(stream);
        return Ok(result);
    }

    /// <summary>
    /// Đồng bộ lại bảng UserRoles dựa trên cột Role của tất cả user.
    /// Gọi API này một lần để fix toàn bộ tài khoản hiện có.
    /// </summary>
    [HttpPost("sync-rbac")]
    [Authorize(Policy = "UserManage")]
    public async Task<ActionResult> SyncAllUserRoles()
    {
        var result = await _userService.SyncAllUserRolesAsync();
        return Ok(result);
    }
}
