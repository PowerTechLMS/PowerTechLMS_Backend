using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/[controller]")]
[Authorize(Policy = "CourseEdit")]
public class ModulesController : ControllerBase
{
    private readonly IModuleService _moduleService;
    public ModulesController(IModuleService moduleService) => _moduleService = moduleService;

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
    private bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Quản trị viên") || User.HasClaim("permission", "user.manage");

    [HttpPost]
    public async Task<ActionResult> Create(int courseId, [FromBody] CreateModuleRequest request)
        => Ok(await _moduleService.CreateModuleAsync(courseId, request, UserId, IsAdmin));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int courseId, int id, [FromBody] UpdateModuleRequest request)
    {
        try { return Ok(await _moduleService.UpdateModuleAsync(courseId, id, request, UserId, IsAdmin)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int courseId, int id)
    {
        try { await _moduleService.DeleteModuleAsync(courseId, id, UserId, IsAdmin); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpPut("sort-order")]
    public async Task<ActionResult> UpdateSortOrder(int courseId, [FromBody] UpdateSortOrderRequest request)
    {
        try
        {
            await _moduleService.UpdateSortOrderAsync(courseId, request.Items, UserId, IsAdmin);
            return Ok();
        }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }
}
