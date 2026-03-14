using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    public CoursesController(ICourseService courseService) => _courseService = courseService;


    private int UserId => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
    private int? CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : null;
        }
    }

    [HttpGet]
    [AllowAnonymous] // hoặc Authorize
    public async Task<ActionResult> GetCourses(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] string? search = null,
    [FromQuery] bool? isPublished = null,
    [FromQuery] int? categoryId = null,
    [FromQuery] int? level = null)
    {
        return Ok(await _courseService.GetCoursesAsync(page, pageSize, search, isPublished, categoryId, CurrentUserId, level));
    }


    [HttpGet("{id}")]
    public async Task<ActionResult> GetCourse(int id)
    {
        var course = await _courseService.GetCourseDetailAsync(id, UserId);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpGet("{id}/preview")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPreview(int id)
    {
        var course = await _courseService.GetCoursePreviewAsync(id, CurrentUserId);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpPost]
    [Authorize(Policy = "CourseCreate")]
    public async Task<ActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        => Ok(await _courseService.CreateCourseAsync(request, UserId));

    [HttpPut("{id}")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        try { return Ok(await _courseService.UpdateCourseAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CourseDelete")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        try { await _courseService.DeleteCourseAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id}/cover")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> UploadCover(int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var url = await _courseService.UploadCoverImageAsync(id, stream, file.FileName);
        return Ok(new { url });
    }

    [HttpGet("{id}/certificate-template")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> GetCertificateTemplate(int id)
    {
        var template = await _courseService.GetCourseCertificateTemplateAsync(id);
        return template == null ? NotFound(new { message = "Chưa có mẫu chứng chỉ được thiết lập cho khóa học này." }) : Ok(template);
    }

    [HttpPut("{id}/certificate-template")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> SaveCertificateTemplate(int id, [FromBody] CertificateTemplateDto request)
    {
        try { return Ok(await _courseService.SaveCourseCertificateTemplateAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
