using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/lessons/{lessonId}/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    public NotesController(INoteService noteService) => _noteService = noteService;

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

    [HttpPost]
    public async Task<ActionResult> Create(int lessonId, [FromBody] CreateNoteRequest request) => Ok(
        await _noteService.CreateNoteAsync(lessonId, UserId, request));

    [HttpGet]
    public async Task<ActionResult> GetAll(int lessonId) => Ok(await _noteService.GetLessonNotesAsync(lessonId, UserId));

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            await _noteService.DeleteNoteAsync(id, UserId);
            return NoContent();
        } catch(Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
