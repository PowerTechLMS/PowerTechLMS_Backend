using LMS.Core.DTOs;
using LMS.Core.Entities;
using System.Security.Claims;

namespace LMS.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);

    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}

public interface IUserService
{
    Task<PagedResponse<UserResponse>> GetUsersAsync(int page, int pageSize, string? search);

    Task ToggleActiveAsync(int userId, int adminId);

    Task<UserResponse> GetUserProfileAsync(int userId);

    Task<UserProfileReportResponse> GetUserProfileReportAsync(int userId);

    Task<UserResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);

    Task UpdateAvatarAsync(int userId, string avatarUrl);

    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);

    Task<UserResponse> UpdateUserAsync(int userId, UpdateUserRequest request);

    Task<UserResponse> CreateUserAsync(UpdateUserRequest request);

    Task<ImportResultResponse> ImportUsersAsync(Stream fileStream);

    Task<object> SyncAllUserRolesAsync();
}

public interface IGroupService
{
    Task<PagedResponse<UserGroupResponse>> GetUserGroupsAsync(int page, int pageSize, string? search);

    Task<UserGroupDetailResponse?> GetUserGroupDetailAsync(int groupId);

    Task<UserGroupResponse> CreateUserGroupAsync(UserGroupRequest request, int adminId);

    Task<UserGroupResponse> UpdateUserGroupAsync(int groupId, UserGroupRequest request);

    Task DeleteUserGroupAsync(int groupId);

    Task AddUserToGroupAsync(int groupId, int userId, int adminId);

    Task RemoveUserFromGroupAsync(int groupId, int userId);

    Task AssignCourseGroupToDepartmentAsync(int departmentId, int courseGroupId, int adminId);

    Task RemoveCourseGroupFromDepartmentAsync(int departmentId, int courseGroupId);

    Task<PagedResponse<CourseGroupResponse>> GetCourseGroupsAsync(int page, int pageSize, string? search);

    Task<CourseGroupDetailResponse?> GetCourseGroupDetailAsync(int groupId, int? userId = null);

    Task<CourseGroupResponse> CreateCourseGroupAsync(CourseGroupRequest request, int adminId);

    Task<CourseGroupResponse> UpdateCourseGroupAsync(int groupId, CourseGroupRequest request);

    Task DeleteCourseGroupAsync(int groupId);

    Task AddCourseToGroupAsync(int groupId, int courseId, int? sortOrder = null);

    Task RemoveCourseFromGroupAsync(int groupId, int courseId);

    Task<List<CourseGroupResponse>> GetMyCourseGroupsAsync(int userId);
}

public interface ICourseService
{
    Task<PagedResponse<CourseResponse>> GetCoursesAsync(
        int page,
        int pageSize,
        string? search,
        bool? isPublished = null,
        int? categoryId = null,
        int? userId = null,
        int? level = null,
        bool isInstructorManagement = false,
        bool isAdmin = false);

    Task<CourseDetailResponse?> GetCourseDetailAsync(int courseId, int userId, bool isAdmin = false);

    Task<CourseDetailResponse?> GetCoursePreviewAsync(int courseId, int? userId = null);

    Task<CourseResponse> CreateCourseAsync(CreateCourseRequest request, int userId);

    Task<CourseResponse> UpdateCourseAsync(int courseId, UpdateCourseRequest request, int userId, bool isAdmin = false);

    Task DeleteCourseAsync(int courseId, int userId, bool isAdmin = false);

    Task<string> UploadCoverImageAsync(
        int courseId,
        Stream fileStream,
        string fileName,
        int userId,
        bool isAdmin = false);

    Task<CertificateTemplateDto?> GetCourseCertificateTemplateAsync(int courseId);

    Task<CertificateTemplateDto> SaveCourseCertificateTemplateAsync(
        int courseId,
        CertificateTemplateDto request,
        int userId,
        bool isAdmin = false);
}

public interface IModuleService
{
    Task<ModuleResponse> CreateModuleAsync(int courseId, CreateModuleRequest request, int userId, bool isAdmin = false);

    Task<ModuleResponse> UpdateModuleAsync(
        int courseId,
        int moduleId,
        UpdateModuleRequest request,
        int userId,
        bool isAdmin = false);

    Task DeleteModuleAsync(int courseId, int moduleId, int userId, bool isAdmin = false);

    Task UpdateSortOrderAsync(int courseId, List<SortOrderItem> items, int userId, bool isAdmin = false);
}

public interface ILessonService
{
    Task<LessonResponse> CreateLessonAsync(int moduleId, CreateLessonRequest request, int userId, bool isAdmin = false);

    Task<int> CreateLessonQuizAsync(int lessonId, CreateQuizRequest request);

    Task<LessonResponse> UpdateLessonAsync(
        int moduleId,
        int lessonId,
        UpdateLessonRequest request,
        int userId,
        bool isAdmin = false);

    Task DeleteLessonAsync(int moduleId, int lessonId, int userId, bool isAdmin = false);

    Task UpdateSortOrderAsync(int moduleId, List<SortOrderItem> items, int userId, bool isAdmin = false);

    Task<string> UploadAttachmentAsync(
        int moduleId,
        int lessonId,
        Stream fileStream,
        string fileName,
        int userId,
        bool isAdmin = false);

    Task<string> UploadVideoAsync(int lessonId, Stream fileStream, string fileName);

    Task UpdateVideoMetadataAsync(int lessonId, string storageKey, string storageUrl);

    Task<(Stream stream, string fileName, string contentType)> GetAttachmentFileAsync(int attachmentId);

    Task DeleteAttachmentAsync(int moduleId, int attachmentId, int userId, bool isAdmin = false);
}

public interface IEnrollmentService
{
    Task<EnrollmentResponse> EnrollAsync(int userId, int courseId);

    Task<EnrollmentResponse> AdminEnrollAsync(AdminEnrollRequest request, int assignedById);

    Task<EnrollmentResponse> ApproveEnrollmentAsync(int enrollmentId, bool approved, string? reason = null);

    Task<List<EnrollmentResponse>> GetUserEnrollmentsAsync(int userId);

    Task<List<EnrollmentResponse>> GetCourseEnrollmentsAsync(int courseId);

    Task<List<EnrollmentResponse>> GetPendingEnrollmentsAsync(int userId, bool isAdmin = false);

    Task<object> GetAllEnrollmentsAsync(int page, int pageSize, int userId, bool isAdmin = false);
}

public interface IProgressService
{
    Task<ProgressResponse> CompleteLessonAsync(int userId, int lessonId, bool isQuizPassed = false);

    Task<ProgressResponse> UpdateVideoPositionAsync(int userId, int lessonId, int positionSeconds, int watchedPercent);

    Task<CourseProgressResponse> GetCourseProgressAsync(int userId, int courseId);

    Task<List<CourseProgressResponse>> GetUserProgressAsync(int userId);

    Task<List<ProgressResponse>> GetLessonProgressesAsync(int userId, int courseId);

    Task<bool> CanAccessLessonAsync(int userId, int lessonId);
}

public interface IQuizService
{
    Task<QuizDetailResponse?> GetQuizDetailAsync(int quizId);

    Task<QuestionResponse> AddQuestionAsync(int quizId, CreateQuestionRequest request);

    Task<Quiz> CreateQuizAsync(int courseId, CreateQuizRequest request);

    Task<StartQuizResponse> StartQuizAsync(int userId, int quizId);

    Task SaveAnswerDraftAsync(int attemptId, int userId, int questionId, string? selected);

    Task UpdateRemainingTimeAsync(int attemptId, int userId, int remainingSeconds);

    Task<QuizResultResponse> SubmitQuizAsync(int userId, int attemptId, SubmitQuizRequest request);

    Task<List<QuizResultResponse>> GetUserQuizResultsAsync(int userId, int quizId);

    Task UpdateQuizAsync(int quizId, CreateQuizRequest request);

    Task UpdateQuestionAsync(int questionId, CreateQuestionRequest request);

    Task DeleteQuestionAsync(int questionId);
}

public interface ICertificateService
{
    Task<CertificateResponse?> IssueCertificateAsync(int userId, int courseId);

    Task<List<CertificateResponse>> GetUserCertificatesAsync(int userId);

    Task<CertificateResponse?> VerifyCertificateAsync(string code, ClaimsPrincipal user);

    Task<PagedResponse<AdminCertificateResponse>> GetCertificatesAsync(
        int page,
        int pageSize,
        string? search,
        ClaimsPrincipal user);

    Task RevokeCertificateAsync(int id, string reason, int adminId);
}

public interface IQAService
{
    Task<QAResponse> CreatePostAsync(int lessonId, int userId, CreateQARequest request);

    Task<List<QAResponse>> GetLessonQAAsync(int lessonId);

    Task DeletePostAsync(int postId, int userId);
}

public interface INoteService
{
    Task<NoteResponse> CreateNoteAsync(int lessonId, int userId, CreateNoteRequest request);

    Task<List<NoteResponse>> GetLessonNotesAsync(int lessonId, int userId);

    Task DeleteNoteAsync(int noteId, int userId);
}

public interface ILeaderboardService
{
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(int top = 10);

    Task<List<LeaderboardResponse>> GetMonthlyLeaderboardAsync();

    Task<List<UserBadgeResponse>> GetUserBadgesAsync(int userId);

    Task CheckAndAwardBadgesAsync(int userId);
}

public interface IDocumentService
{
    Task<PagedResponse<DocumentResponse>> GetDocumentsAsync(
        int page,
        int pageSize,
        string? search,
        string? tag,
        bool isAdmin = false,
        int? userId = null,
        bool isInstructorManagement = false);

    Task<DocumentResponse> CreateDocumentAsync(
        CreateDocumentRequest request,
        int userId,
        Stream fileStream,
        string fileName,
        long fileSize);

    Task<DocumentResponse> UpdateDocumentAsync(
        int documentId,
        UpdateDocumentRequest request,
        int userId,
        bool isAdmin = false);

    Task<DocumentResponse> AddVersionAsync(
        int documentId,
        int userId,
        Stream fileStream,
        string fileName,
        long fileSize,
        string? changeNote,
        bool isAdmin = false);

    Task<List<DocumentVersionResponse>> GetVersionsAsync(int documentId);

    Task<(Stream stream, string fileName, string contentType)> GetFileAsync(int documentId);

    Task<(Stream stream, string fileName, string contentType)> GetVersionFileAsync(int versionId);

    Task DeleteDocumentAsync(int documentId, int userId, bool isAdmin = false);

    Task<DocumentConfigResponse?> GetDocumentConfigAsync(int documentId);

    Task<List<DocumentPermissionResponse>> GetDocumentPermissionsAsync(int documentId);

    Task UpdateDocumentPermissionsAsync(int documentId, UpdatePermissionRequest request);

    Task ClearDocumentPermissionsAsync(int documentId);
}

public interface IReportService
{
    Task<List<TrainingReportResponse>> GetTrainingReportAsync(int? courseId, int userId, bool isAdmin = false);

    Task<List<InactiveUserResponse>> GetInactiveUsersAsync(int days = 30, int? userId = null, bool isAdmin = false);

    Task<List<QuizAnalyticsResponse>> GetQuizAnalyticsAsync(int quizId, int userId, bool isAdmin = false);
}

public record RoleDto(int Id, string Name, string? Description, bool IsSystem, List<string> Permissions);

public record UserRoleDto(int UserId, string UserName, string Email, List<string> Roles, string? Avatar = null);

public record AssignPermissionsRequest(List<int> PermissionIds);

public record AssignRolesRequest(List<int> RoleIds);

public record CreateRoleRequest(string Name, string? Description);

public interface IRbacService
{
    Task<List<RoleDto>> GetRolesAsync();

    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);

    Task<RoleDto> UpdateRolePermissionsAsync(int roleId, AssignPermissionsRequest request);

    Task DeleteRoleAsync(int roleId);

    Task<List<PermissionDto>> GetPermissionsAsync();

    Task<UserRoleDto> GetUserRolesAsync(int userId);

    Task<UserRoleDto> UpdateUserRolesAsync(int userId, AssignRolesRequest request);
}

public record PermissionDto(int Id, string Code, string Name, string? Category);

public interface IDashboardService
{
    Task<LearnerDashboardResponse> GetLearnerDashboardAsync(int userId);
}

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);

    void QueueEmail(string to, string subject, string body);
}

public interface IMailQueue
{
    void Enqueue(MailJob job);

    Task<MailJob> DequeueAsync(CancellationToken cancellationToken);
}