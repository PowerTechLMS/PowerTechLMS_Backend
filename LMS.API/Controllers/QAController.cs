using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/lessons/{lessonId}/[controller]")]
[Authorize]
public class QAController : ControllerBase
{
    private readonly IQAService _qaService;
    public QAController(IQAService qaService) => _qaService = qaService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<ActionResult> Create(int lessonId, [FromBody] CreateQARequest request)
        => Ok(await _qaService.CreatePostAsync(lessonId, UserId, request));

    [HttpGet]
    public async Task<ActionResult> GetAll(int lessonId)
        => Ok(await _qaService.GetLessonQAAsync(lessonId));

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _qaService.DeletePostAsync(id, UserId); return NoContent(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}
