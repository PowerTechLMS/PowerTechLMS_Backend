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

    private int? CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : null;
        }
    }

    private bool IsAdmin => User.IsInRole("Admin") ||
        User.IsInRole("Quản trị viên") ||
        User.HasClaim("permission", "user.manage");

    private bool IsInstructor => User.IsInRole("Instructor") || User.IsInRole("Giảng viên");

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? level = null,
        [FromQuery] bool manage = false)
    {
        bool isInstructorManagement = manage && (IsAdmin || IsInstructor);
        return Ok(
            await _courseService.GetCoursesAsync(
                page,
                pageSize,
                search,
                isPublished,
                categoryId,
                CurrentUserId,
                level,
                isInstructorManagement,
                IsAdmin));
    }


    [HttpGet("{id}")]
    public async Task<ActionResult> GetCourse(int id)
    {
        var course = await _courseService.GetCourseDetailAsync(id, UserId, IsAdmin);
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
    public async Task<ActionResult> CreateCourse([FromBody] CreateCourseRequest request) => Ok(
        await _courseService.CreateCourseAsync(request, UserId));

    [HttpPut("{id}")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request)
    {
        try
        {
            return Ok(await _courseService.UpdateCourseAsync(id, request, UserId, IsAdmin));
        } catch(KeyNotFoundException)
        {
            return NotFound();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CourseDelete")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        try
        {
            await _courseService.DeleteCourseAsync(id, UserId, IsAdmin);
            return NoContent();
        } catch(KeyNotFoundException)
        {
            return NotFound();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPost("{id}/cover")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> UploadCover(int id, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var url = await _courseService.UploadCoverImageAsync(id, stream, file.FileName, UserId, IsAdmin);
            return Ok(new { url });
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}/certificate-template")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> GetCertificateTemplate(int id)
    {
        var template = await _courseService.GetCourseCertificateTemplateAsync(id);
        return template == null
            ? NotFound(new { message = "Chưa có mẫu chứng chỉ được thiết lập cho khóa học này." })
            : Ok(template);
    }

    [HttpPut("{id}/certificate-template")]
    [Authorize(Policy = "CourseEdit")]
    public async Task<ActionResult> SaveCertificateTemplate(int id, [FromBody] CertificateTemplateDto request)
    {
        try
        {
            return Ok(await _courseService.SaveCourseCertificateTemplateAsync(id, request, UserId, IsAdmin));
        } catch(KeyNotFoundException)
        {
            return NotFound();
        } catch(UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
