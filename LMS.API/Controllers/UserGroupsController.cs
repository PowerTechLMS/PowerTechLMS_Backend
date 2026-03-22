using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserGroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    public UserGroupsController(IGroupService groupService) => _groupService = groupService;

    private int AdminId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    [Authorize(Policy = "GroupView")]
    public async Task<ActionResult> GetGroups(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null) => Ok(await _groupService.GetUserGroupsAsync(page, pageSize, search));

    [HttpGet("{id}")]
    [Authorize(Policy = "GroupView")]
    public async Task<ActionResult> GetGroup(int id)
    {
        var group = await _groupService.GetUserGroupDetailAsync(id);
        return group == null ? NotFound() : Ok(group);
    }

    [HttpPost]
    [Authorize(Policy = "GroupManage")]
    public async Task<ActionResult> Create([FromBody] UserGroupRequest request) => Ok(
        await _groupService.CreateUserGroupAsync(request, AdminId));

    [HttpPut("{id}")]
    [Authorize(Policy = "GroupManage")]
    public async Task<ActionResult> Update(int id, [FromBody] UserGroupRequest request)
    {
        try
        {
            return Ok(await _groupService.UpdateUserGroupAsync(id, request));
        } catch(KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "GroupManage")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _groupService.DeleteUserGroupAsync(id);
            return NoContent();
        } catch(KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{groupId}/users/{userId}")]
    [Authorize(Policy = "GroupManage")]
    public async Task<ActionResult> AddUser(int groupId, int userId)
    {
        try
        {
            await _groupService.AddUserToGroupAsync(groupId, userId, AdminId);
            return Ok();
        } catch(InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{groupId}/users/{userId}")]
    [Authorize(Policy = "GroupManage")]
    public async Task<ActionResult> RemoveUser(int groupId, int userId)
    {
        try
        {
            await _groupService.RemoveUserFromGroupAsync(groupId, userId);
            return NoContent();
        } catch(KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/course-groups/{courseGroupId}")]
    public async Task<ActionResult> AssignCourseGroup(int id, int courseGroupId)
    {
        try
        {
            await _groupService.AssignCourseGroupToDepartmentAsync(id, courseGroupId, AdminId);
            return Ok(new { message = "Gán khung đào tạo thành công" });
        } catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}/course-groups/{courseGroupId}")]
    public async Task<ActionResult> RemoveCourseGroup(int id, int courseGroupId)
    {
        try
        {
            await _groupService.RemoveCourseGroupFromDepartmentAsync(id, courseGroupId);
            return NoContent();
        } catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
