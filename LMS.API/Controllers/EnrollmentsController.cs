using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    public EnrollmentsController(IEnrollmentService enrollmentService) => _enrollmentService = enrollmentService;

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub")
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("UserId");
            if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }
    private bool IsAdmin => User.IsInRole("Admin") || User.IsInRole("Quản trị viên") || User.HasClaim("permission", "user.manage");

    [HttpGet]
    [Authorize] // Yêu cầu có token đăng nhập
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5000)
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync(page, pageSize, UserId, IsAdmin);
        return Ok(result);
    }
    [HttpPost]
    public async Task<ActionResult> Enroll([FromBody] EnrollRequest request)
    {
        try { return Ok(await _enrollmentService.EnrollAsync(UserId, request.CourseId)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("admin")]
    [Authorize(Policy = "EnrollmentAssign")]
    public async Task<ActionResult> AdminEnroll([FromBody] AdminEnrollRequest request)
    {
        try { return Ok(await _enrollmentService.AdminEnrollAsync(request, UserId)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("{id}/approve")]
    [Authorize(Policy = "EnrollmentApprove")]
    public async Task<ActionResult> Approve(int id, [FromBody] ApproveEnrollmentRequest request)
    {
        try { return Ok(await _enrollmentService.ApproveEnrollmentAsync(id, request.Approved)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("my")]
    public async Task<ActionResult> GetMyEnrollments()
        => Ok(await _enrollmentService.GetUserEnrollmentsAsync(UserId));

    [HttpGet("course/{courseId}")]
    [Authorize(Policy = "EnrollmentView")]
    public async Task<ActionResult> GetCourseEnrollments(int courseId)
        => Ok(await _enrollmentService.GetCourseEnrollmentsAsync(courseId));

    [HttpGet("pending")]
    [Authorize(Policy = "EnrollmentView")]
    public async Task<ActionResult> GetPending()
        => Ok(await _enrollmentService.GetPendingEnrollmentsAsync(UserId, IsAdmin));
}
