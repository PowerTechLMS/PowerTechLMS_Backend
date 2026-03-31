
namespace LMS.Core.DTOs;

public record LoginRequest(string Email, string Password);

public record RegisterRequest(string FullName, string Email, string Password, string? Role);

public record MailJob(string To, string Subject, string Body, int RetryCount = 0);

public record AuthResponse(
    int Id,
    string FullName,
    string Email,
    string Role,
    string Token,
    List<string> Roles,
    List<string> Permissions,
    string? Avatar = null);

public record UserResponse(
    int Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    string? Phone,
    string? Address,
    string? Bio,
    string? Avatar,
    DateTime CreatedAt,
    string? GroupName = null,
    int? GroupId = null,
    string? Position = null);

public record UpdateProfileRequest(string FullName, string? Phone, string? Address, string? Bio, string? Avatar);

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record UserProfileReportResponse(
    int Enrolled,
    int Completed,
    int Certificates,
    List<UserCourseProgressDto> Courses);

public record UserCourseProgressDto(int Id, string Title, double Progress, string Status, DateTime? LastAccess);

public record UpdateUserRequest(
    string FullName,
    string Email,
    string Role,
    int? GroupId,
    bool IsActive,
    string? Password,
    string? Position
);

public class UserImportRow
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? Role { get; set; }
}

public record ImportResultResponse(int SuccessCount, List<string> Errors);

public record UserGroupRequest(string Name, string? Description);

public record UserGroupResponse(
    int Id,
    string Name,
    string? Description,
    int MemberCount,
    int CourseGroupCount,
    DateTime CreatedAt
);

public record UserGroupMemberResponse(
    int UserId,
    string FullName,
    string Email,
    string Role,
    bool IsActive,
    string? Avatar,
    DateTime AddedAt);

public record UserGroupDetailResponse(
    int Id,
    string Name,
    string? Description,
    List<UserGroupMemberResponse> Members,
    List<int> CourseGroupIds,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsDeleted);

public record CourseGroupRequest(string Name, string? Description);

public record CourseGroupResponse(
    int Id,
    string Name,
    string? Description,
    int CourseCount,
    int DepartmentCount,
    DateTime CreatedAt,
    double ProgressPercent = 0,
    int PendingCertCount = 0,
    int TotalHours = 0
);

public record CourseGroupDetailResponse(
    int Id,
    string Name,
    string? Description,
    List<CourseResponse> Courses,
    DateTime CreatedAt,
    double ProgressPercent = 0,
    int TotalHours = 0);

public record CreateCourseRequest(
    string Title,
    string Description,
    int PassScore = 8,
    bool IsPublished = false,
    DateTime? EnrollStartDate = null,
    DateTime? EnrollEndDate = null,
    int? CategoryId = null,
    int? CompletionDeadlineDays = null,
    DateTime? CompletionEndDate = null,
    int Level = 3,
    int QuizRetakeWaitTimeMinutes = 5,
    int QuizMaxRetakesPerDay = 3,
    int? UserGroupId = null);

public record UpdateCourseRequest(
    string Title,
    string Description,
    int PassScore,
    bool IsPublished,
    DateTime? EnrollStartDate = null,
    DateTime? EnrollEndDate = null,
    int? CategoryId = null,
    int? CompletionDeadlineDays = null,
    DateTime? CompletionEndDate = null,
    int Level = 3,
    int QuizRetakeWaitTimeMinutes = 5,
    int QuizMaxRetakesPerDay = 3,
    int? UserGroupId = null);

public record CourseResponse(
    int Id,
    string Title,
    string Description,
    string? CoverImageUrl,
    bool IsPublished,
    int PassScore,
    string CreatedByName,
    DateTime CreatedAt,
    int ModuleCount,
    int LessonCount,
    int EnrollmentCount,
    DateTime? EnrollStartDate,
    DateTime? EnrollEndDate,
    int? CompletionDeadlineDays,
    DateTime? CompletionEndDate,
    bool RequiresApproval,
    int? FinalQuizId,
    int? CategoryId,
    string? CategoryName,
    int Level,
    int QuizRetakeWaitTimeMinutes,
    int QuizMaxRetakesPerDay,
    int? DepartmentId = null,
    string? DepartmentName = null
);

public record CourseDetailResponse(
    int Id,
    string Title,
    string Description,
    string? CoverImageUrl,
    bool IsPublished,
    int PassScore,
    string CreatedByName,
    DateTime CreatedAt,
    List<ModuleResponse> Modules,
    int EnrollmentCount,
    DateTime? EnrollStartDate,
    DateTime? EnrollEndDate,
    int? CompletionDeadlineDays,
    DateTime? CompletionEndDate,
    bool RequiresApproval,
    int? FinalQuizId,
    int? CategoryId,
    string? CategoryName,
    int Level,
    List<QuizSummaryResponse>? ExtraQuizzes = null,
    int QuizRetakeWaitTimeMinutes = 5,
    int QuizMaxRetakesPerDay = 3,
    int? DepartmentId = null,
    string? DepartmentName = null
);

public record QuizSummaryResponse(int Id, string Title, int QuestionCount = 0);


public record QuizDetailResponse(
    int Id,
    string Title,
    int? TimeLimitMinutes,
    int PassScore,
    int QuestionCount,
    List<QuestionBankResponse> Questions,
    int? RetakeWaitTimeMinutes = null,
    int? MaxRetakesPerDay = null
);

public record QuestionBankResponse(
    int Id,
    string QuestionText,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectAnswer,
    decimal Points,
    string? Explanation = null
);

public record CertificateTemplateDto(
    bool UseBuiltInTemplate,
    string? HtmlTemplate,
    string? BackgroundImageUrl,
    string? LogoUrl,
    string? SignatureImageUrl,
    string? SignatureName,
    string? SignatureTitle,
    string TitleText,
    string? BodyText,
    string? FooterText,
    string PrimaryColor
);

public record CreateModuleRequest(string Title, int SortOrder);

public record UpdateModuleRequest(string Title, int SortOrder);

public record ModuleResponse(int Id, string Title, int SortOrder, List<LessonResponse> Lessons);

public record CreateLessonRequest(
    string Title,
    string Type,
    string? Content,
    string? VideoUrl,
    int SortOrder,
    bool IsFreePreview,
    int VideoDurationSeconds = 0,
    int ReadingDurationSeconds = 0,
    string VideoStatus = "Ready");

public record UpdateLessonRequest(
    string Title,
    string Type,
    string? Content,
    string? VideoUrl,
    int SortOrder,
    bool IsFreePreview,
    int VideoDurationSeconds,
    int ReadingDurationSeconds = 0,
    string VideoStatus = "Ready");

public record LessonResponse(
    int Id,
    string Title,
    string Type,
    string? Content,
    string? VideoUrl,
    int VideoDurationSeconds,
    int ReadingDurationSeconds,
    string VideoStatus,
    int SortOrder,
    bool IsFreePreview,
    List<AttachmentResponse> Attachments,
    int? QuizId = null,
    int QuizQuestionCount = 0,
    string? AiSummary = null
);

public record AttachmentResponse(int Id, string FileName, long FileSize);

public record EnrollRequest(int CourseId);

public record AdminEnrollRequest(int UserId, int CourseId, DateTime? Deadline, bool IsMandatory);

public record ApproveEnrollmentRequest(bool Approved, string? Reason = null);

public record EnrollmentResponse(
    int Id,
    int UserId,
    string FullName,
    string Email,
    string? AvatarUrl,
    int CourseId,
    string CourseTitle,
    string Status,
    DateTime? Deadline,
    bool IsMandatory,
    DateTime EnrolledAt,
    double ProgressPercent,
    bool IsOverdue,
    int TotalLessons,
    int CompletedLessons,
    bool IsLocked = false,
    int CourseLevel = 3,
    int? GroupEnrollId = null,
    string? DepartmentName = null,
    string? CoverImageUrl = null,
    string? RejectionReason = null
);


public record CompleteLessonRequest(int LessonId, bool IsQuizPassed = false);

public record UpdateVideoPositionRequest(int LessonId, int PositionSeconds, int WatchedPercent);

public record ProgressResponse(
    int LessonId,
    bool IsCompleted,
    int VideoPositionSeconds,
    int VideoWatchedPercent,
    bool IsQuizPassed = false,
    bool HasQuiz = false);

public record CourseProgressResponse(
    int CourseId,
    string CourseTitle,
    double ProgressPercent,
    int CompletedLessons,
    int TotalLessons,
    int PassedQuizzes,
    int TotalQuizzes,
    bool IsCompleted,
    bool QuizPassed,
    List<int>? PassedQuizIds = null);

public record CreateQuizRequest(
    string Title,
    int? TimeLimitMinutes,
    int PassScore,
    int QuestionCount,
    bool ShuffleQuestions,
    bool ShuffleAnswers,
    int? MaxAttemptsPerWindow = null,
    int? AttemptWindowHours = null,
    int? AvailableFromDays = null,
    DateTime? QuizStartDate = null,
    DateTime? QuizEndDate = null,
    int? RetakeWaitTimeMinutes = null,
    int? MaxRetakesPerDay = null);

public class CreateQuestionRequest
{
    public string Content { get; set; } = string.Empty;

    public string OptionA { get; set; } = string.Empty;

    public string OptionB { get; set; } = string.Empty;

    public string OptionC { get; set; } = string.Empty;

    public string OptionD { get; set; } = string.Empty;

    public string CorrectAnswer { get; set; } = "A";

    public decimal Points { get; set; } = 1m;

    public string? Explanation { get; set; }
}

public record QuestionResponse(
    int Id,
    string Content,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD,
    string CorrectAnswer,
    double Points
);

public record StartQuizResponse(
    int AttemptId,
    int? TimeLimitMinutes,
    DateTime StartedAt,
    List<QuizQuestionResponse> Questions,
    Dictionary<int, string>? DraftAnswers = null,
    int? RemainingSeconds = null,
    int RemainingAttemptsToday = 0);

public record QuizQuestionResponse(
    int QuestionId,
    string QuestionText,
    string OptionA,
    string OptionB,
    string OptionC,
    string OptionD);

public record SubmitQuizRequest(List<QuizAnswerRequest> Answers);

public record QuizAnswerRequest(int QuestionId, string SelectedAnswer);

public record SaveDraftRequest(int QuestionId, string? SelectedAnswer);

public record UpdateTimeRequest(int RemainingSeconds);

public record QuizResultResponse(
    int AttemptId,
    decimal Score,
    int CorrectAnswers,
    int TotalQuestions,
    bool IsPassed,
    List<QuizAnswerDetailResponse> Details,
    int RemainingAttemptsToday = 0,
    int WaitMinutes = 0);

public record QuizAnswerDetailResponse(
    int QuestionId,
    string QuestionText,
    string SelectedAnswer,
    string CorrectAnswer,
    bool IsCorrect,
    string? Explanation = null);

public record CertificateResponse(
    int Id,
    string FullName,
    string CourseTitle,
    string CertificateCode,
    string? PdfUrl,
    DateTime IssuedAt,
    string Status,
    DateTime? RevokedAt);

public record AdminCertificateResponse(
    int Id,
    string StudentName,
    string CertificateCode,
    string CourseName,
    double Score,
    DateTime IssueDate,
    string Status,
    DateTime? RevokedAt);

public record RevokeCertificateRequest(string Reason);

public record CreateQARequest(string Content, int? ParentId);

public record QAResponse(
    int Id,
    string Content,
    string FullName,
    string? UserAvatar,
    int? ParentId,
    DateTime CreatedAt,
    List<QAResponse> Replies);

public record CreateNoteRequest(string Content, int? VideoTimestampSeconds);

public record NoteResponse(int Id, string Content, int? VideoTimestampSeconds, DateTime CreatedAt);

public record BadgeResponse(
    int Id,
    string Name,
    string Description,
    string? IconUrl,
    int RequiredCourses,
    bool IsEarned,
    DateTime? EarnedAt);

public record CreateDocumentRequest(
    string Title,
    string? Description,
    string? Tags,
    DateTime? AccessStartDate = null,
    DateTime? AccessEndDate = null);

public record UpdateDocumentRequest(
    string Title,
    string? Description,
    string? Tags,
    DateTime? AccessStartDate = null,
    DateTime? AccessEndDate = null);

public record AddDocumentVersionRequest(string? ChangeNote);

public record DocumentResponse(
    int Id,
    string Title,
    string? Description,
    string FileName,
    long FileSize,
    string? Tags,
    string UploadedByName,
    DateTime CreatedAt,
    int CurrentVersionNumber = 1,
    DateTime? AccessStartDate = null,
    DateTime? AccessEndDate = null);

public record DocumentVersionResponse(
    int Id,
    int DocumentId,
    int VersionNumber,
    string FileName,
    long FileSize,
    string? FileType,
    string? ChangeNote,
    string UploadedByName,
    DateTime CreatedAt);

public class UpdatePermissionRequest
{
    public List<int>? RoleIds { get; set; }

    public List<int>? GroupIds { get; set; }

    public List<int>? UserIds { get; set; }
}

public record DocumentPermissionResponse(
    int Id,
    int DocumentId,
    int? RoleId,
    int? UserGroupId,
    int? UserId
);

public record DocumentConfigResponse(
    int Id,
    string Title,
    string? Description,
    string? Tags,
    DateTime? AccessStartDate,
    DateTime? AccessEndDate
);

public record TrainingReportResponse(
    int UserId,
    string FullName,
    string Email,
    int CourseId,
    string CourseTitle,
    double ProgressPercent,
    string Status,
    DateTime? Deadline,
    bool IsOverdue,
    DateTime EnrolledAt);

public record InactiveUserReport(
    int UserId,
    string FullName,
    string Email,
    int CourseId,
    string CourseTitle,
    DateTime EnrolledAt,
    int DaysSinceEnroll);

public record LeaderboardResponse(int Rank, int UserId, string UserName, string? Avatar, int LessonsCompleted);

public record UserBadgeResponse(int BadgeId, string BadgeName, string Description, string? IconUrl, DateTime EarnedAt);

public record InactiveUserResponse(
    int UserId,
    string FullName,
    string Email,
    int CourseId,
    string CourseTitle,
    DateTime LastActiveDate,
    int InactiveDays);

public record QuizAnalyticsResponse(
    int QuestionId,
    string QuestionText,
    int TotalAttempts,
    int WrongAnswers,
    double ErrorRate);

public record LeaderboardEntry(
    int Rank,
    int UserId,
    string UserName,
    string? Avatar,
    int CompletedCourses,
    int TotalScore,
    List<BadgeResponse> Badges);

public record UpdateSortOrderRequest(List<SortOrderItem> Items);

public record SortOrderItem(int Id, int SortOrder);

public record PagedResponse<T>(List<T> Items, int TotalCount, int Page, int PageSize);

public record EnrollmentProgressResponse(int UserId, int CourseId, string Status, double ProgressPercent);

public class LearnerDashboardResponse
{
    public string DepartmentName { get; set; } = "Chưa có bộ phận";

    public List<DashboardTaskDto> TodayTasks { get; set; } = new();

    public WeeklyMissionDto WeekMission { get; set; } = new();

    public List<DashboardCourseDto> MyCourses { get; set; } = new();

    public List<DashboardQuizDto> TestPractice { get; set; } = new();

    public LearningProfileDto LearningProfile { get; set; } = new();

    public List<HrMessageDto> HrMessages { get; set; } = new();
}

public record DashboardTaskDto(int Id, string Title, string Sub, string Progress, bool Done, bool Locked);

public record SkillProgressDto(string Name, int Pct, string Color);

public class WeeklyMissionDto
{
    public string WeekLabel { get; set; } = string.Empty;

    public int TotalTasks { get; set; }

    public int DoneTasks { get; set; }

    public int MandatoryDone { get; set; }

    public int MandatoryTotal { get; set; }

    public int OptionalDone { get; set; }

    public int OptionalTotal { get; set; }

    public int XpEarned { get; set; }

    public int XpTarget { get; set; }

    public int Streak { get; set; }

    public List<SkillProgressDto> Skills { get; set; } = new();

    public string Status { get; set; } = string.Empty;

    public string StatusType { get; set; } = "ok";
}

public record DashboardCourseDto(
    int Id,
    string Title,
    string Tag,
    string Units,
    string Cups,
    string ThumbColor,
    string ThumbColor2,
    double ProgressPercent,
    int CompletedLessons,
    int TotalLessons,
    string? NextLessonTitle = null,
    string? NextLessonDuration = null,
    string? CoverImageUrl = null);


public record DashboardQuizDto(int Id, string Title, string Subtitle, string Status, string Badge, string Cover);

public class LearningProfileDto
{
    public string Dept { get; set; } = string.Empty;

    public string Level { get; set; } = "Newbie";

    public string Entry { get; set; } = "Nhân viên";

    public string Predicted { get; set; } = "Chuyên viên";

    public string Target { get; set; } = "Trưởng nhóm";

    public double EntryScore { get; set; } = 3.0;

    public double PredictedScore { get; set; } = 0;

    public double TargetScore { get; set; } = 5.0;

    public List<SummaryStatDto> Summary { get; set; } = new();
}

public record SummaryStatDto(string Label, string Val, string Color);

public record HrMessageDto(int Id, string From, string Msg, string Time, bool Unread);
