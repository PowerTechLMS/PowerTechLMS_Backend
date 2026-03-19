using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _docService;

    public DocumentsController(IDocumentService docService) => _docService = docService;

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
    private bool IsInstructor => User.IsInRole("Instructor") || User.IsInRole("Giảng viên");

    [HttpGet]
    public async Task<ActionResult> GetAll(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20, 
        [FromQuery] string? search = null, 
        [FromQuery] string? tag = null,
        [FromQuery] bool manage = false)
    {
        bool isInstructorManagement = manage && (IsAdmin || IsInstructor);
        return Ok(await _docService.GetDocumentsAsync(page, pageSize, search, tag, IsAdmin, UserId, isInstructorManagement));
    }

    [HttpPost]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> Create([FromForm] CreateDocumentRequest request, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        return Ok(await _docService.CreateDocumentAsync(request, UserId, stream, file.FileName, file.Length));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> Update(int id, [FromBody] UpdateDocumentRequest request)
    {
        try { return Ok(await _docService.UpdateDocumentAsync(id, request, UserId, IsAdmin)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "DocDelete")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _docService.DeleteDocumentAsync(id, UserId, IsAdmin); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
    }

    [HttpPost("{id}/versions")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> AddVersion(int id, [FromForm] AddDocumentVersionRequest request, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            return Ok(await _docService.AddVersionAsync(id, UserId, stream, file.FileName, file.Length, request.ChangeNote, IsAdmin));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult> GetVersions(int id)
    {
        return Ok(await _docService.GetVersionsAsync(id));
    }

    [HttpGet("versions/{versionId}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadVersion(int versionId, [FromQuery] bool preview = false)
    {
        try
        {
            var (stream, fileName, contentType) = await _docService.GetVersionFileAsync(versionId);
            if (preview) return File(stream, contentType);
            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (FileNotFoundException) { return NotFound(new { message = "File vật lý không tồn tại" }); }
    }

    [HttpGet("{id}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(int id, [FromQuery] bool preview = false)
    {
        try
        {
            var (stream, fileName, contentType) = await _docService.GetFileAsync(id);
            if (preview) return File(stream, contentType);
            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (FileNotFoundException) { return NotFound("Tệp vật lý không còn trên máy chủ."); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var doc = await _docService.GetDocumentConfigAsync(id);
        return doc == null ? NotFound() : Ok(doc);
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult> GetPermissions(int id)
    {
        return Ok(await _docService.GetDocumentPermissionsAsync(id));
    }

    [HttpPut("{id}/permissions")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> UpdatePermissions(int id, [FromBody] UpdatePermissionRequest req)
    {
        await _docService.UpdateDocumentPermissionsAsync(id, req);
        return Ok(new { message = "Đã cập nhật quyền" });
    }

    [HttpDelete("{id}/permissions")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> ClearPermissions(int id)
    {
        await _docService.ClearDocumentPermissionsAsync(id);
        return NoContent();
    }
}
