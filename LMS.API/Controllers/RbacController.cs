using System.Security.Claims;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RoleManage")]
public class RbacController : ControllerBase
{
    private readonly IRbacService _rbacService;
    public RbacController(IRbacService rbacService) => _rbacService = rbacService;

    // ===== Roles =====

    [HttpGet("roles")]
    public async Task<ActionResult> GetRoles()
        => Ok(await _rbacService.GetRolesAsync());

    [HttpPost("roles")]
    public async Task<ActionResult> CreateRole([FromBody] CreateRoleRequest request)
        => Ok(await _rbacService.CreateRoleAsync(request));

    [HttpPut("roles/{id}/permissions")]
    public async Task<ActionResult> UpdateRolePermissions(int id, [FromBody] AssignPermissionsRequest request)
    {
        try { return Ok(await _rbacService.UpdateRolePermissionsAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("roles/{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        try { await _rbacService.DeleteRoleAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // ===== Permissions =====

    [HttpGet("permissions")]
    public async Task<ActionResult> GetPermissions()
        => Ok(await _rbacService.GetPermissionsAsync());

    // ===== User Roles =====

    [HttpGet("users/{id}/roles")]
    public async Task<ActionResult> GetUserRoles(int id)
    {
        try { return Ok(await _rbacService.GetUserRolesAsync(id)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("users/{id}/roles")]
    public async Task<ActionResult> UpdateUserRoles(int id, [FromBody] AssignRolesRequest request)
    {
        try { return Ok(await _rbacService.UpdateUserRolesAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
