using System.Security.Claims;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Core.DTOs;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "UserManage")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    private int AdminId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => Ok(await _userService.GetUsersAsync(page, pageSize, search));

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserProfileAsync(id);
            return Ok(user);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    // Lưu ý: Đổi chữ 'UpdateUserRequest' thành tên DTO/Model tương ứng của bạn nếu khác
    {
        try
        {
            // Gọi hàm update từ Service của bạn (tên hàm có thể khác tùy bạn đặt trong IUserService)
            await _userService.UpdateUserAsync(id, request);
            return Ok(new { message = "Cập nhật thành công!" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy người dùng." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}/report")]
    public async Task<ActionResult> GetUserReport(int id)
    {
        try
        {
            var report = await _userService.GetUserProfileReportAsync(id);
            return Ok(report);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("{id}/toggle-active")]
    public async Task<ActionResult> ToggleActive(int id)
    {
        try
        {
            await _userService.ToggleActiveAsync(id, AdminId);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
