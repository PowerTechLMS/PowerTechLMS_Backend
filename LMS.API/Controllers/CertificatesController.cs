using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certService;
    public CertificatesController(ICertificateService certService) => _certService = certService;

    private int UserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("UserId")
                     ?? User.FindFirst("sub");

            if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }

    [HttpPost("{courseId}")]
    public async Task<ActionResult> IssueCertificate(int courseId)
    {
        try
        {
            var cert = await _certService.IssueCertificateAsync(UserId, courseId);
            if (cert == null) return BadRequest(new { message = "Bạn chưa đủ điều kiện nhận chứng chỉ (Cần hoàn thành bài học và đạt bài tập)." });
            return Ok(cert);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult> GetMyCertificates()
        => Ok(await _certService.GetUserCertificatesAsync(UserId));

    [HttpGet("verify/{code}")]
    public async Task<ActionResult> Verify(string code)
    {
        // [BUSINESS RULE]: Học viên khi chưa đăng nhập thì không có phép xem (Authorize class level đã chặn 401)
        var cert = await _certService.VerifyCertificateAsync(code, User);
        return cert == null ? NotFound(new { message = "Chứng chỉ không tồn tại hoặc bạn không có quyền xem." }) : Ok(cert);
    }

    [HttpGet("admin")]
    [Authorize(Policy = "CertificateView")]
    public async Task<ActionResult> GetAdminCertificates([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        return Ok(await _certService.GetCertificatesAsync(page, pageSize, search, User));
    }

    [HttpPut("{id}/revoke")]
    [Authorize(Policy = "CertificateManage")]
    public async Task<ActionResult> RevokeCertificate(int id, [FromBody] RevokeCertificateRequest request)
    {
        try 
        { 
            await _certService.RevokeCertificateAsync(id, request.Reason, UserId); 
            return Ok(); 
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
