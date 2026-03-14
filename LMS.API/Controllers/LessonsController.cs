using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/modules/{moduleId}/[controller]")]
[Authorize(Policy = "CourseEdit")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
        => _lessonService = lessonService;

    [HttpPost]
    public async Task<ActionResult> Create(int moduleId, [FromBody] CreateLessonRequest request)
        => Ok(await _lessonService.CreateLessonAsync(moduleId, request));

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateLessonRequest request)
    {
        try { return Ok(await _lessonService.UpdateLessonAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _lessonService.DeleteLessonAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("sort-order")]
    public async Task<ActionResult> UpdateSortOrder([FromBody] UpdateSortOrderRequest request)
    {
        await _lessonService.UpdateSortOrderAsync(request.Items);
        return Ok();
    }

    [HttpPost("{lessonId}/attachments")]
    public async Task<ActionResult> UploadAttachment(int moduleId, int lessonId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "Tệp đính kèm không hợp lệ." });
        using var stream = file.OpenReadStream();
        return Ok(await _lessonService.UploadAttachmentAsync(lessonId, stream, file.FileName, file.Length));
    }

    [HttpPost("{id}/video")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult> UploadVideo(int moduleId, int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var url = await _lessonService.UploadVideoAsync(id, stream, file.FileName);
        return Ok(new { url });
    }

    [HttpDelete("attachments/{attachmentId}")]
    public async Task<ActionResult> DeleteAttachment(int attachmentId)
    {
        try { await _lessonService.DeleteAttachmentAsync(attachmentId); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id}/quiz")]
    public async Task<ActionResult> CreateLessonQuiz(int moduleId, int id, [FromBody] CreateQuizRequest request)
    {
        try
        {
            var quizId = await _lessonService.CreateLessonQuizAsync(id, request);
            return Ok(new { id = quizId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy bài học để thêm Quiz." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("attachments/{attachmentId}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadAttachment(int moduleId, int attachmentId)
    {
        try
        {
            var (stream, fileName, contentType) = await _lessonService.GetAttachmentFileAsync(attachmentId);
            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Không tìm thấy tài liệu đính kèm." });
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "Tệp vật lý không còn trên máy chủ." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
