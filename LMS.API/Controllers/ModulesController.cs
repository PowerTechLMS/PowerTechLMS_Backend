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

    [HttpPost]
    public async Task<ActionResult> Create(int courseId, [FromBody] CreateModuleRequest request)
        => Ok(await _moduleService.CreateModuleAsync(courseId, request));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateModuleRequest request)
    {
        try { return Ok(await _moduleService.UpdateModuleAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _moduleService.DeleteModuleAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("sort-order")]
    public async Task<ActionResult> UpdateSortOrder([FromBody] UpdateSortOrderRequest request)
    { await _moduleService.UpdateSortOrderAsync(request.Items); return Ok(); }
}
