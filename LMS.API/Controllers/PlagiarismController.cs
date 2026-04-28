using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlagiarismController : ControllerBase
{
    private readonly IPlagiarismService _plagiarismService;

    public PlagiarismController(IPlagiarismService plagiarismService)
    {
        _plagiarismService = plagiarismService;
    }

    [HttpPost("compare")]
    public async Task<ActionResult<PlagiarismCompareResponse>> Compare([FromBody] PlagiarismCompareRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await _plagiarismService.CompareAsync(request);
        return Ok(result);
    }

    [HttpGet("scan/{lessonId}")]
    public async Task<ActionResult<List<PlagiarismScanResponse>>> ScanLesson(
        int lessonId,
        [FromQuery] double threshold = 40.0)
    {
        var result = await _plagiarismService.ScanLessonAsync(lessonId, threshold);
        return Ok(result);
    }
}
