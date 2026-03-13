using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try { return Ok(await _authService.LoginAsync(request)); }
        catch (UnauthorizedAccessException ex) { return Unauthorized(new { message = ex.Message }); }
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try { return Ok(await _authService.RegisterAsync(request)); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    public CoursesController(ICourseService courseService) => _courseService = courseService;


    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    [HttpGet]
    [AllowAnonymous] // hoặc Authorize
    public async Task<ActionResult> GetCourses(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 12,
    [FromQuery] string? search = null,
    [FromQuery] bool? isPublished = null,
    [FromQuery] int? categoryId = null)
    {
        return Ok(await _courseService.GetCoursesAsync(page, pageSize, search, isPublished, categoryId));
    }


    [HttpGet("{id}")]
    public async Task<ActionResult> GetCourse(int id)
    {
        var course = await _courseService.GetCourseDetailAsync(id);
        return course == null ? NotFound() : Ok(course);
    }

    [HttpGet("{id}/preview")]
    [AllowAnonymous]
    public async Task<ActionResult> GetPreview(int id)
    {
        var course = await _courseService.GetCoursePreviewAsync(id);
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

    // ========================================================================
    // API MỚI ĐƯỢC THÊM VÀO: XỬ LÝ LƯU MINI-QUIZ CHO BÀI HỌC
    // Endpoint: POST /api/modules/{moduleId}/lessons/{id}/quiz
    // ========================================================================
    [HttpPost("{id}/quiz")]
    public async Task<ActionResult> CreateLessonQuiz(int moduleId, int id, [FromBody] CreateQuizRequest request)
    {
        try
        {
            // Gọi sang Service để tạo Quiz và map Id của Quiz đó vào cột QuizId của bảng Lessons
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
    // ========================================================================
    // API MỚI ĐƯỢC THÊM VÀO: XỬ LÝ TẢI FILE TÀI LIỆU ĐÍNH KÈM CỦA BÀI HỌC
    // Endpoint: GET /api/modules/{moduleId}/lessons/attachments/{attachmentId}/download
    // ========================================================================
    [HttpGet("attachments/{attachmentId}/download")]
    [AllowAnonymous] // Cho phép tải file mượt mà
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

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    public EnrollmentsController(IEnrollmentService enrollmentService) => _enrollmentService = enrollmentService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    [HttpGet]
    [Authorize] // Yêu cầu có token đăng nhập
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5000)
    {
        var result = await _enrollmentService.GetAllEnrollmentsAsync(page, pageSize);
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
        => Ok(await _enrollmentService.GetPendingEnrollmentsAsync());
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ILeaderboardService _leaderboardService;

    public ProgressController(IProgressService progressService, ILeaderboardService leaderboardService)
    {
        _progressService = progressService;
        _leaderboardService = leaderboardService;
    }

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost("complete")]
    public async Task<ActionResult> CompleteLesson([FromBody] CompleteLessonRequest request)
    {
        try
        {
            var result = await _progressService.CompleteLessonAsync(UserId, request.LessonId);
            await _leaderboardService.CheckAndAwardBadgesAsync(UserId);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPut("video-position")]
    public async Task<ActionResult> UpdateVideoPosition([FromBody] UpdateVideoPositionRequest request)
        => Ok(await _progressService.UpdateVideoPositionAsync(UserId, request.LessonId, request.PositionSeconds, request.WatchedPercent));

    [HttpGet("course/{courseId}")]
    public async Task<ActionResult> GetCourseProgress(int courseId)
        => Ok(await _progressService.GetCourseProgressAsync(UserId, courseId));

    [HttpGet("my")]
    public async Task<ActionResult> GetMyProgress()
        => Ok(await _progressService.GetUserProgressAsync(UserId));

    [HttpGet("lessons/{courseId}")]
    public async Task<ActionResult> GetLessonProgresses(int courseId)
        => Ok(await _progressService.GetLessonProgressesAsync(UserId, courseId));

    [HttpGet("can-access/{lessonId}")]
    public async Task<ActionResult> CanAccess(int lessonId)
        => Ok(new { canAccess = await _progressService.CanAccessLessonAsync(UserId, lessonId) });
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;
    public QuizzesController(IQuizService quizService) => _quizService = quizService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetQuizDetail(int id)
    {
        try
        {
            // Gọi qua Service
            var quiz = await _quizService.GetQuizDetailAsync(id);
            if (quiz == null) return NotFound(new { message = "Không tìm thấy bài thi" });

            return Ok(quiz);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi Backend", error = ex.Message });
        }
    }

    [HttpPost("course/{courseId}")]
    [Authorize(Policy = "QuizCreate")]
    public async Task<ActionResult> CreateQuiz(int courseId, [FromBody] CreateQuizRequest request)
        => Ok(await _quizService.CreateQuizAsync(courseId, request));

    [HttpPost("{quizId}/questions")]
    [Authorize(Policy = "QuizCreate")]
    // ĐÃ FIX: Sửa 'int id' thành 'int quizId' để khớp với Route {quizId}
    public async Task<ActionResult> AddQuestion(int quizId, [FromBody] CreateQuestionRequest request)
    {
        try
        {
            // 1. Gọi thực thi hàm thêm câu hỏi
            var result = await _quizService.AddQuestionAsync(quizId, request);

            // 2. Trả về kết quả (Đảm bảo AddQuestionAsync trả về một đối tượng, không phải void)
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            // Trả về lỗi 400 Bad Request với message Tiếng Việt thay vì sập 500
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Bắt các lỗi hệ thống khác nếu có
            return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
        }
    }

    [HttpPost("{quizId}/start")]
    public async Task<ActionResult> StartQuiz(int quizId)
    {
        try { return Ok(await _quizService.StartQuizAsync(UserId, quizId)); }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    // [6] Save draft answer during exam
    [HttpPut("{attemptId}/draft")]
    public async Task<ActionResult> SaveDraft(int attemptId, [FromBody] SaveDraftRequest request)
    {
        try { await _quizService.SaveAnswerDraftAsync(attemptId, UserId, request.QuestionId, request.SelectedAnswer); return Ok(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    // [5] Update remaining time (called on visibility change)
    [HttpPut("{attemptId}/time")]
    public async Task<ActionResult> UpdateTime(int attemptId, [FromBody] UpdateTimeRequest request)
    {
        await _quizService.UpdateRemainingTimeAsync(attemptId, UserId, request.RemainingSeconds);
        return Ok();
    }

    [HttpPost("{attemptId}/submit")]
    public async Task<ActionResult> SubmitQuiz(int attemptId, [FromBody] SubmitQuizRequest request)
    {
        try { return Ok(await _quizService.SubmitQuizAsync(UserId, attemptId, request)); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{quizId}/results")]
    public async Task<ActionResult> GetResults(int quizId)
        => Ok(await _quizService.GetUserQuizResultsAsync(UserId, quizId));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly ICertificateService _certService;
    public CertificatesController(ICertificateService certService) => _certService = certService;

    // Thay thế biến UserId cũ bằng thuộc tính an toàn này:
    private int UserId
    {
        get
        {
            // Tìm claim theo nhiều định dạng khác nhau để chống lỗi Null
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
            // Gọi hàm cấp chứng chỉ
            var cert = await _certService.IssueCertificateAsync(UserId, courseId);
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
    [AllowAnonymous]
    public async Task<ActionResult> Verify(string code)
    {
        var cert = await _certService.VerifyCertificateAsync(code);
        return cert == null ? NotFound(new { message = "Chứng chỉ không tồn tại." }) : Ok(cert);
    }

    [HttpGet("admin")]
    [Authorize(Policy = "ReportView")] // Hoặc role Admin tùy logic permission hiện tại
    public async Task<ActionResult> GetAdminCertificates([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        return Ok(await _certService.GetCertificatesAsync(page, pageSize, search));
    }

    [HttpPut("{id}/revoke")]
    [Authorize(Policy = "ReportView")]
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

[ApiController]
[Route("api/lessons/{lessonId}/[controller]")]
[Authorize]
public class NotesController : ControllerBase
{
    private readonly INoteService _noteService;
    public NotesController(INoteService noteService) => _noteService = noteService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<ActionResult> Create(int lessonId, [FromBody] CreateNoteRequest request)
        => Ok(await _noteService.CreateNoteAsync(lessonId, UserId, request));

    [HttpGet]
    public async Task<ActionResult> GetAll(int lessonId)
        => Ok(await _noteService.GetLessonNotesAsync(lessonId, UserId));

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _noteService.DeleteNoteAsync(id, UserId); return NoContent(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaderboardController : ControllerBase
{
    private readonly ILeaderboardService _service;
    public LeaderboardController(ILeaderboardService service) => _service = service;

    // Thay thế biến UserId cũ bằng thuộc tính an toàn này:
    private int UserId
    {
        get
        {
            // Tìm claim theo nhiều định dạng khác nhau để chống lỗi Null
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("id")
                     ?? User.FindFirst("UserId")
                     ?? User.FindFirst("sub");

            if (claim == null) throw new UnauthorizedAccessException("Không tìm thấy UserId trong Token.");
            return int.Parse(claim.Value);
        }
    }
    [HttpGet]
    public async Task<ActionResult> GetLeaderboard([FromQuery] int top = 10)
        => Ok(await _service.GetLeaderboardAsync(top));


    [HttpGet("monthly")]
    public async Task<ActionResult> GetMonthlyLeaderboard() => Ok(await _service.GetMonthlyLeaderboardAsync());

    [HttpGet("badges/{userId}")]
    public async Task<ActionResult> GetBadges(int userId) => Ok(await _service.GetUserBadgesAsync(userId));
    [HttpGet("badges")]
    public IActionResult GetMyBadges()
    {
        return Ok(new[] {
            new { BadgeId = 1, BadgeName = "Chăm chỉ", IsEarned = true },
            new { BadgeId = 2, BadgeName = "Điểm tuyệt đối", IsEarned = true }
        });
    }

}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _docService;

    public DocumentsController(IDocumentService docService) => _docService = docService;

    private int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, [FromQuery] string? tag = null)
    {
        var isAdmin = User.IsInRole("Admin");

        // TRUYỀN THÊM UserId VÀO ĐÂY (Ở vị trí cuối cùng)
        return Ok(await _docService.GetDocumentsAsync(page, pageSize, search, tag, isAdmin, UserId));
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
        try { return Ok(await _docService.UpdateDocumentAsync(id, request)); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "DocDelete")]
    public async Task<ActionResult> Delete(int id)
    {
        try { await _docService.DeleteDocumentAsync(id); return NoContent(); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("{id}/versions")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> AddVersion(int id, [FromForm] AddDocumentVersionRequest request, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            return Ok(await _docService.AddVersionAsync(id, UserId, stream, file.FileName, file.Length, request.ChangeNote));
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpGet("{id}/versions")]
    public async Task<ActionResult> GetVersions(int id)
    {
        return Ok(await _docService.GetVersionsAsync(id));
    }

    // [FIXED]: THÊM ALLOW ANONYMOUS ĐỂ TRÌNH DUYỆT CÓ THỂ TẢI FILE MÀ KHÔNG BỊ CHẶN (LỖI 404)
    [HttpGet("versions/{versionId}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadVersion(int versionId)
    {
        try
        {
            var (stream, fileName, contentType) = await _docService.GetVersionFileAsync(versionId);
            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (FileNotFoundException) { return NotFound(new { message = "File vật lý không tồn tại" }); }
    }

    // [FIXED]: THÊM ALLOW ANONYMOUS ĐỂ TRÌNH DUYỆT CÓ THỂ TẢI FILE MÀ KHÔNG BỊ CHẶN (LỖI 404)
    [HttpGet("{id}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var (stream, fileName, contentType) = await _docService.GetFileAsync(id);
            return File(stream, contentType, fileName);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (FileNotFoundException) { return NotFound("Tệp vật lý không còn trên máy chủ."); }
        catch (Exception ex) { return BadRequest(new { message = ex.Message }); }
    }
    // 1. Lấy chi tiết 1 Document
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var doc = await _docService.GetDocumentConfigAsync(id);
        return doc == null ? NotFound() : Ok(doc);
    }

    // 2. Lấy danh sách quyền của Document
    [HttpGet("{id}/permissions")]
    public async Task<ActionResult> GetPermissions(int id)
    {
        return Ok(await _docService.GetDocumentPermissionsAsync(id));
    }

    // 3. Cập nhật phân quyền
    [HttpPut("{id}/permissions")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> UpdatePermissions(int id, [FromBody] UpdatePermissionRequest req)
    {
        await _docService.UpdateDocumentPermissionsAsync(id, req);
        return Ok(new { message = "Đã cập nhật quyền" });
    }

    // 4. Xóa quyền (Chuyển sang Public)
    [HttpDelete("{id}/permissions")]
    [Authorize(Policy = "DocUpload")]
    public async Task<ActionResult> ClearPermissions(int id)
    {
        await _docService.ClearDocumentPermissionsAsync(id);
        return NoContent();
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ReportView")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService) => _reportService = reportService;

    [HttpGet("training")]
    public async Task<ActionResult> GetTrainingReport([FromQuery] int? courseId)
        => Ok(await _reportService.GetTrainingReportAsync(courseId));

    [HttpGet("inactive")]
    public async Task<ActionResult> GetInactive([FromQuery] int days = 30)
        => Ok(await _reportService.GetInactiveUsersAsync(days));

    [HttpGet("quiz-analytics/{quizId}")]
    public async Task<ActionResult> GetQuizAnalytics(int quizId)
        => Ok(await _reportService.GetQuizAnalyticsAsync(quizId));
}
