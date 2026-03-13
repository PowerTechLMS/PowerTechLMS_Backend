using System.Security.Claims;
using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "GroupManage")]
public class CourseGroupsController : ControllerBase
{
    private readonly IGroupService _groupService;
    public CourseGroupsController(IGroupService groupService) => _groupService = groupService;

    private int AdminId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult> GetGroups([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
        => Ok(await _groupService.GetCourseGroupsAsync(page, pageSize, search));

    [HttpGet("{id}")]
    public async Task<ActionResult> GetGroup(int id)
    {
        var group = await _groupService.GetCourseGroupDetailAsync(id);
        return group == null ? NotFound() : Ok(group);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CourseGroupRequest request)
        => Ok(await _groupService.CreateCourseGroupAsync(request, AdminId));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] CourseGroupRequest request)
    {
        try { return Ok(await _groupService.UpdateCourseGroupAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _groupService.DeleteCourseGroupAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{groupId}/courses/{courseId}")]
    public async Task<ActionResult> AddCourse(int groupId, int courseId, [FromQuery] int? sortOrder = null)
    {
        try
        {
            await _groupService.AddCourseToGroupAsync(groupId, courseId, sortOrder);
            return Ok();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpDelete("{groupId}/courses/{courseId}")]
    public async Task<ActionResult> RemoveCourse(int groupId, int courseId)
    {
        try
        {
            await _groupService.RemoveCourseFromGroupAsync(groupId, courseId);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
