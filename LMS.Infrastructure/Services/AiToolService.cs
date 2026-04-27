using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LMS.Infrastructure.Services;

public class AiToolService : IAiToolService
{
    private readonly AppDbContext _db;
    private readonly ICourseService _courseService;
    private readonly IUserService _userService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGroupService _groupService;
    private readonly IEmailService _emailService;
    private readonly ILlmService _llmService;
    private readonly VectorDbService _vectorDb;
    private readonly IImageGenerationService _imageService;

    public AiToolService(
        AppDbContext db,
        ICourseService courseService,
        IUserService userService,
        IEnrollmentService enrollmentService,
        IGroupService groupService,
        IEmailService emailService,
        ILlmService llmService,
        VectorDbService vectorDb,
        IImageGenerationService imageService)
    {
        _db = db;
        _courseService = courseService;
        _userService = userService;
        _enrollmentService = enrollmentService;
        _groupService = groupService;
        _emailService = emailService;
        _llmService = llmService;
        _vectorDb = vectorDb;
        _imageService = imageService;
    }

    private static readonly List<AiToolInfo> _allTools = new()
    {
        new AiToolInfo(
        "analyze_performance",
        "Phân tích hiệu suất học tập của người dùng. Dùng khi hỏi về kết quả, điểm số.",
        new[] { "report.view" }),
        new AiToolInfo(
        "get_user_ai_learning_history",
        "Lấy lịch sử học tập AI (Role-play, Essay) của người dùng.",
        new[] { "user.view" }),
        new AiToolInfo("search_courses", "Tìm kiếm các khóa học theo từ khóa.", new[] { "course.view" }),
        new AiToolInfo(
        "search_users_departments",
        "Tìm kiếm thành viên hoặc phòng ban.",
        new[] { "user.view", "enrollment.assign" }),
        new AiToolInfo(
        "search_vector_content",
        "Tìm kiếm nội dung ngữ nghĩa trong Vector DB (Bài giảng, Tài liệu).",
        new[] { "course.view" }),
        new AiToolInfo(
        "get_course_students",
        "Lấy danh sách tất cả học viên đã đăng ký một khóa học.",
        new[] { "course.view" }),
        new AiToolInfo(
        "get_course_details",
        "Lấy chi tiết khóa học, module và danh sách bài học.",
        new[] { "course.view" }),
        new AiToolInfo("update_course", "Cập nhật tiêu đề hoặc mô tả khóa học.", new[] { "course.edit" }),
        new AiToolInfo(
        "generate_lesson_content",
        "Sinh nội dung bài giảng mới dựa trên chủ đề.",
        new[] { "course.edit" }),
        new AiToolInfo("create_new_course", "Tạo một khóa học mới dưới dạng bản nháp.", new[] { "course.create" }),
        new AiToolInfo("mass_enroll_users", "Ghi danh hàng loạt học viên vào khóa học.", new[] { "enrollment.assign" }),
        new AiToolInfo(
        "generate_infographic",
        "Tạo hình ảnh infographic tóm tắt nội dung bài học từ Vector DB.",
        new[] { "course.view" }),
        new AiToolInfo("send_email_report", "Gửi email báo cáo kết quả thực hiện cho Admin.", new string[] { }),
        new AiToolInfo("notify_progress", "Thông báo tiến độ thực hiện tác vụ (System).", new string[] { }),
        new AiToolInfo("register_tasks", "Đăng ký lộ trình thực hiện kế hoạch (System).", new string[] { })
    };

    public async Task<List<AiToolInfo>> GetAvailableToolsAsync(int adminId, string? query = null)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == adminId);

        if(user is null)
            return new List<AiToolInfo>();

        var userPermissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
            .ToHashSet();

        bool isAdmin = user.UserRoles.Any(ur => ur.Role.Name == "Admin");

        return _allTools.Where(
            t => isAdmin || t.Permissions.Length == 0 || t.Permissions.Any(p => userPermissions.Contains(p)))
            .ToList();
    }

    public async Task<AiToolResponse> ExecuteToolAsync(string toolName, string argumentsJson, int adminId)
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var doc = JsonSerializer.Deserialize<JsonElement>(argumentsJson);

            return toolName switch
            {
                "analyze_performance" => new AiToolResponse(
                    true,
                    "Success",
                    await AnalyzePerformanceAsync(
                        (doc.TryGetProperty("query", out var q) ? q.GetString() : null) ??
                            (doc.TryGetProperty("topic", out var t) ? t.GetString() : string.Empty) ??
                            string.Empty,
                        GetIntSafe(doc, "limit") ?? 50)),
                "get_user_ai_learning_history" => await GetUserHistoryInternalAsync(argumentsJson),
                "search_courses" => await SearchEntitiesInternalAsync(argumentsJson, "course"),
                "search_users_departments" => await SearchUsersInternalAsync(argumentsJson),
                "search_vector_content" => await SearchVectorInternalAsync(argumentsJson),
                "get_course_students" => await GetCourseStudentsInternalAsync(argumentsJson),
                "get_course_details" => await GetCourseDetailsInternalAsync(argumentsJson),
                "update_course" => await UpdateCourseInternalAsync(argumentsJson),
                "generate_lesson_content" => await GenerateLessonInternalAsync(argumentsJson),
                "create_new_course" => await CreateCourseInternalAsync(argumentsJson, adminId),
                "mass_enroll_users" => await MassEnrollInternalAsync(argumentsJson),
                "generate_infographic" => await GenerateInfographicInternalAsync(argumentsJson, adminId),
                "assign_users_to_group" => await AssignGroupInternalAsync(argumentsJson),
                "send_email_report" => await SendReportInternalAsync(argumentsJson, adminId),
                "notify_progress" => await NotifyProgressInternalAsync(argumentsJson),
                _ => new AiToolResponse(false, $"Tool '{toolName}' không tồn tại.")
            };
        } catch(Exception ex)
        {
            return new AiToolResponse(false, $"Lỗi thực thi tool: {ex.Message}");
        }
    }

    private int? GetIntSafe(JsonElement doc, string prop)
    {
        if(doc.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.Number)
            return el.GetInt32();
        return null;
    }

    private string? GetStringSafe(JsonElement doc, string prop)
    {
        if(doc.TryGetProperty(prop, out var el) && el.ValueKind == JsonValueKind.String)
            return el.GetString();
        return null;
    }

    private async Task<AiToolResponse> GetCourseStudentsInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var courseId = doc.GetProperty("courseId").GetInt32();
        var students = await _db.Enrollments
            .Include(e => e.User)
            .Where(e => e.CourseId == courseId && !e.IsDeleted)
            .Select(e => new { e.User.Id, e.User.FullName, e.User.Email, e.Status, e.CreatedAt })
            .ToListAsync();
        return new AiToolResponse(true, "Thành công", students);
    }

    private async Task<AiToolResponse> GetUserHistoryInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var userId = doc.GetProperty("userId").GetInt32();
        var data = await GetUserAILearningHistoryAsync(userId);
        return new AiToolResponse(true, "Thành công", data);
    }

    private async Task<AiToolResponse> SearchEntitiesInternalAsync(string json, string typeHint)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var keyword = doc.GetProperty("keyword").GetString()!;
        var type = typeHint ?? doc.GetProperty("entity_type").GetString()!;
        var limit = GetIntSafe(doc, "limit") ?? 50;
        var data = await SearchEntitiesAsync(type, keyword, limit);
        return new AiToolResponse(true, "Thành công", data);
    }

    private async Task<AiToolResponse> SearchUsersInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var keyword = doc.GetProperty("keyword").GetString()!;
        var type = doc.TryGetProperty("entity_type", out var t) ? t.GetString()! : "user";
        var limit = GetIntSafe(doc, "limit") ?? 50;
        var data = await SearchEntitiesAsync(type, keyword, limit);
        return new AiToolResponse(true, "Thành công", data);
    }

    private async Task<AiToolResponse> SearchVectorInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var query = doc.GetProperty("query").GetString()!;
        var topK = doc.TryGetProperty("topK", out var t) ? t.GetInt32() : 5;
        var data = await SearchVectorContentAsync(query, topK);
        return new AiToolResponse(true, "Thành công", data);
    }

    private async Task<AiToolResponse> GetCourseDetailsInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var courseId = doc.GetProperty("courseId").GetInt32();
        return await GetCourseDetailsAsync(courseId);
    }

    private async Task<AiToolResponse> UpdateCourseInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var courseId = doc.GetProperty("courseId").GetInt32();
        var title = doc.TryGetProperty("title", out var t) ? t.GetString() : null;
        var desc = doc.TryGetProperty("description", out var d) ? d.GetString() : null;
        return await UpdateCourseContentAsync(courseId, title, desc);
    }

    private async Task<AiToolResponse> GenerateLessonInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var moduleId = doc.GetProperty("moduleId").GetInt32();
        var topic = doc.GetProperty("topic").GetString()!;
        var type = doc.GetProperty("type").GetString()!;
        return await GenerateLessonContentAsync(moduleId, topic, type);
    }

    public async Task<object> AnalyzePerformanceAsync(string topic, int limit = 50)
    {
        var courses = await _db.Courses
            .Where(c => c.Title.Contains(topic) && !c.IsDeleted)
            .Select(c => c.Id)
            .ToListAsync();

        if(!courses.Any())
            return new { Message = "Không tìm thấy khóa học nào liên quan đến chủ đề này." };

        var topPerformers = await _db.Enrollments
            .Include(e => e.User)
            .Where(e => courses.Contains(e.CourseId) && !e.IsDeleted)
            .GroupBy(e => new { e.UserId, e.User.FullName, e.User.Email })
            .Select(g => new { g.Key.UserId, g.Key.FullName, g.Key.Email, CourseCount = g.Count(), Score = 100 })
            .OrderByDescending(x => x.CourseCount)
            .Take(limit)
            .ToListAsync();

        return topPerformers;
    }

    public async Task<object> SearchEntitiesAsync(string type, string query, int limit = 50)
    {
        return type.ToLower() switch
        {
            "course" => await _db.Courses
                .Where(c => c.Title.Contains(query))
                .Select(c => new { c.Id, c.Title, c.Level })
                .Take(limit)
                .ToListAsync(),
            "user" => await _db.Users
                .Where(u => u.FullName.Contains(query) || u.Email.Contains(query))
                .Select(u => new { u.Id, u.FullName, u.Email, u.Position })
                .Take(limit)
                .ToListAsync(),
            "group" => await _db.UserGroups
                .Where(g => g.Name.Contains(query))
                .Select(g => new { g.Id, g.Name })
                .Take(limit)
                .ToListAsync(),
            _ => new List<object>()
        };
    }

    public async Task<object> GetUserAILearningHistoryAsync(int userId)
    {
        var rolePlaySessions = await _db.RolePlaySessions
            .Include(s => s.Messages)
            .Include(s => s.Lesson)
            .Include(s => s.User)
            .Where(s => s.UserId == userId && s.Lesson != null)
            .OrderByDescending(s => s.CreatedAt)
            .Take(5)
            .Select(
                s => new
                {
                    s.Id,
                    s.UserId,
                    UserFullName = s.User.FullName,
                    UserEmail = s.User.Email,
                    s.LessonId,
                    LessonTitle = s.Lesson.Title,
                    s.Status,
                    s.Score,
                    s.Feedback,
                    Messages = s.Messages.OrderBy(m => m.CreatedAt).Select(m => new { m.Role, m.Content }),
                    s.CreatedAt
                })
            .ToListAsync();

        var essayAttempts = await _db.EssayAttempts
            .Include(a => a.Answers)
            .Include(a => a.Lesson)
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.Lesson != null)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(
                a => new
                {
                    a.Id,
                    a.UserId,
                    UserFullName = a.User.FullName,
                    UserEmail = a.User.Email,
                    a.LessonId,
                    LessonTitle = a.Lesson.Title,
                    a.Status,
                    a.TotalScore,
                    a.AiFeedback,
                    Answers = a.Answers.Select(ans => new { ans.QuestionId, ans.Content, ans.AiScore, ans.AiFeedback }),
                    a.CreatedAt
                })
            .ToListAsync();

        return new { RolePlay = rolePlaySessions, Essays = essayAttempts };
    }

    public async Task<object> SearchVectorContentAsync(string query, int topK = 5)
    {
        return new AiToolResponse(
            true,
            "Tính năng tìm kiếm ngữ nghĩa đang được tích hợp qua Qdrant.",
            new { Query = query, Results = new List<string>() });
    }

    public async Task<AiToolResponse> GetCourseDetailsAsync(int courseId)
    {
        var course = await _db.Courses
            .Include(c => c.Modules)
            .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if(course is null)
            return new AiToolResponse(false, "Không tìm thấy khóa học.");

        return new AiToolResponse(true, "Thành công", course);
    }

    public async Task<AiToolResponse> UpdateCourseContentAsync(int courseId, string? title, string? description)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if(course is null)
            return new AiToolResponse(false, "Không tìm thấy khóa học.");

        if(!string.IsNullOrWhiteSpace(title))
            course.Title = title;
        if(!string.IsNullOrWhiteSpace(description))
            course.Description = description;

        await _db.SaveChangesAsync();
        return new AiToolResponse(true, "Cập nhật khóa học thành công.");
    }

    public async Task<AiToolResponse> GenerateLessonContentAsync(int moduleId, string topic, string type)
    {
        return new AiToolResponse(
            true,
            $"Đang khởi tạo tiến trình sinh nội dung [{type}] cho chủ đề: {topic}",
            new { ModuleId = moduleId });
    }

    public async Task<AiToolResponse> CreateCourseAsync(string title, int? categoryId, int level, int adminId)
    {
        var request = new CreateCourseRequest(
            Title: title,
            Description: $"Khóa học {title} được tạo bởi AI Agent",
            PassScore: 8,
            IsPublished: false,
            CategoryId: categoryId,
            Level: level);

        var result = await _courseService.CreateCourseAsync(request, adminId);
        return new AiToolResponse(true, "Khóa học đã được tạo dưới dạng bản nháp.", result);
    }

    private async Task<AiToolResponse> CreateCourseInternalAsync(string json, int adminId)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var title = GetStringSafe(doc, "title") ?? throw new Exception("Thiếu title");
        var level = GetIntSafe(doc, "level") ?? 3;
        var catId = GetIntSafe(doc, "categoryId");

        var request = new CreateCourseRequest(
            Title: title,
            Description: $"Khóa học {title} được tạo bởi AI Agent",
            PassScore: 8,
            IsPublished: false,
            CategoryId: catId,
            Level: level);

        var res = await _courseService.CreateCourseAsync(request, adminId);
        return new AiToolResponse(true, "Thành công", res);
    }

    public async Task<AiToolResponse> MassEnrollAsync(List<int> userIds, int courseId, int adminId)
    {
        int success = 0;
        foreach(var uid in userIds)
        {
            try
            {
                var request = new AdminEnrollRequest(uid, courseId, null, true);
                await _enrollmentService.AdminEnrollAsync(request, adminId);
                success++;
            } catch
            {
            }
        }
        return new AiToolResponse(true, $"Đã ghi danh thành công {success}/{userIds.Count} học viên.");
    }

    private async Task<AiToolResponse> MassEnrollInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var userIds = doc.GetProperty("userIds").EnumerateArray().Select(x => x.GetInt32()).ToList();
        var courseId = GetIntSafe(doc, "courseId") ?? 0;
        var adminId = GetIntSafe(doc, "adminId") ?? 0;
        return await MassEnrollAsync(userIds, courseId, adminId > 0 ? adminId : 2);
    }

    public async Task<AiToolResponse> AssignGroupAsync(List<int> userIds, int groupId)
    {
        int success = 0;
        foreach(var uid in userIds)
        {
            try
            {
                var existing = await _db.UserGroupMembers.AnyAsync(ug => ug.UserId == uid && ug.GroupId == groupId);
                if(!existing)
                {
                    _db.UserGroupMembers.Add(new UserGroupMember { UserId = uid, GroupId = groupId });
                    success++;
                }
            } catch
            {
            }
        }
        await _db.SaveChangesAsync();
        return new AiToolResponse(true, $"Đã gán {success}/{userIds.Count} học viên vào nhóm.");
    }

    private async Task<AiToolResponse> AssignGroupInternalAsync(string json)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var userIds = doc.GetProperty("userIds").EnumerateArray().Select(x => x.GetInt32()).ToList();
        var groupId = GetIntSafe(doc, "groupId") ?? 0;
        return await AssignGroupAsync(userIds, groupId);
    }

    private async Task<AiToolResponse> SendReportInternalAsync(string json, int adminId)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var subject = doc.GetProperty("subject").GetString()!;
        var body = doc.GetProperty("body").GetString()!;
        var toEmail = doc.TryGetProperty("toEmail", out var te) ? te.GetString() : null;

        string targetEmail = string.Empty;
        if(!string.IsNullOrWhiteSpace(toEmail))
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == toEmail);
            if(!exists)
                return new AiToolResponse(false, $"Email '{toEmail}' không tồn tại trong hệ thống.");
            targetEmail = toEmail;
        } else
        {
            var admin = await _db.Users.FindAsync(adminId);
            if(admin is null || string.IsNullOrEmpty(admin.Email))
                return new AiToolResponse(false, "Không tìm thấy email của Admin.");
            targetEmail = admin.Email;
        }

        var htmlBody = BuildPremiumHtmlEmail(subject, body);
        await _emailService.SendEmailAsync(targetEmail, subject, htmlBody);
        return new AiToolResponse(true, "Báo cáo đã được gửi thành công.");
    }

    private string BuildPremiumHtmlEmail(string title, string content)
    {
        var formattedContent = content
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(
                line =>
                {
                    line = line.Trim();
                    if(string.IsNullOrEmpty(line))
                        return "<br/>";
                    if(Regex.IsMatch(line, @"^(\d+\.|[\*\-])"))
                    {
                        return $"<li style='margin-bottom: 8px;'>{line}</li>";
                    }
                    return $"<p style='margin-bottom: 15px; color: #4b5563; line-height: 1.6;'>{line}</p>";
                })
            .Aggregate((a, b) => a + b);

        if(formattedContent.Contains("<li"))
        {
            formattedContent = Regex.Replace(
                formattedContent,
                @"(<li.*?>.*?</li>)+",
                match => $"<ul style='margin-bottom: 20px; padding-left: 20px; color: #4b5563;'>{match.Value}</ul>");
        }

        return $@"
        <html>
        <body style='margin: 0; padding: 0; font-family: ""Inter"", sans-serif; background-color: #f3f4f6;'>
            <table width='100%' border='0' cellspacing='0' cellpadding='0' style='background-color: #f3f4f6; padding: 40px 20px;'>
                <tr>
                    <td align='center'>
                        <table width='600' border='0' cellspacing='0' cellpadding='0' style='background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);'>
                            <!-- Header -->
                            <tr>
                                <td style='background: linear-gradient(135deg, #4f46e5 0%, #3b82f6 100%); padding: 40px; text-align: center;'>
                                    <h1 style='margin: 0; color: #ffffff; font-size: 28px; font-weight: 800; letter-spacing: -0.025em;'>PowerTech LMS</h1>
                                    <p style='margin: 10px 0 0 0; color: rgba(255, 255, 255, 0.9); font-size: 16px;'>Hệ thống quản trị thông minh AI</p>
                                </td>
                            </tr>
                            <!-- Body -->
                            <tr>
                                <td style='padding: 40px;'>
                                    <h2 style='margin: 0 0 20px 0; color: #111827; font-size: 22px; font-weight: 700;'>{title}</h2>
                                    <div style='font-size: 16px; color: #374151;'>
                                        {formattedContent}
                                    </div>
                                    <div style='margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb;'>
                                        <p style='margin: 0; font-size: 14px; color: #6b7280;'>Trân trọng,<br><b>Đội ngũ hỗ trợ PowerTech</b></p>
                                    </div>
                                </td>
                            </tr>
                            <!-- Footer -->
                            <tr>
                                <td style='background-color: #f9fafb; padding: 20px; text-align: center; border-top: 1px solid #f3f4f6;'>
                                    <p style='margin: 0; font-size: 12px; color: #9ca3af;'>&copy; 2026 PowerTech. All rights reserved.</p>
                                    <p style='margin: 5px 0 0 0; font-size: 12px; color: #9ca3af;'>Đây là email tự động, vui lòng không phản hồi.</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";
    }

    private async Task<AiToolResponse> GenerateInfographicInternalAsync(string json, int adminId)
    {
        var doc = JsonSerializer.Deserialize<JsonElement>(json);
        var lessonIds = new List<int>();

        if(doc.TryGetProperty("lessonIds", out var idsProp) && idsProp.ValueKind == JsonValueKind.Array)
        {
            foreach(var id in idsProp.EnumerateArray())
            {
                lessonIds.Add(id.GetInt32());
            }
        } else if(doc.TryGetProperty("lessonId", out var idProp))
        {
            lessonIds.Add(idProp.GetInt32());
        }

        if(!lessonIds.Any())
        {
            return new AiToolResponse(false, "Vui lòng chọn ít nhất một bài học.");
        }

        return await GenerateInfographicAsync(lessonIds, adminId);
    }

    public async Task<AiToolResponse> GenerateInfographicAsync(IEnumerable<int> lessonIds, int adminId)
    {
        var lessons = await _db.Lessons.Where(l => lessonIds.Contains(l.Id)).ToListAsync();

        if(!lessons.Any())
            return new AiToolResponse(false, "Không tìm thấy bất kỳ bài học nào hợp lệ.");

        var allSegments = new List<(string Content, string Metadata)>();
        foreach(var lessonId in lessonIds)
        {
            var segments = await _vectorDb.GetAllSegmentsAsync(lessonId);
            allSegments.AddRange(segments);
        }

        if(!allSegments.Any())
            return new AiToolResponse(
                false,
                "Các bài học được chọn chưa có dữ liệu ngữ nghĩa (Vector Data) để tạo Infographic.");

        var combinedContent = string.Join("\n\n", allSegments.Select(s => s.Content));

        var summaryPrompt = @"Bạn là một chuyên gia thiết kế Infographic. 
Hãy tóm tắt nội dung các bài học sau đây thành một bản phác thảo chi tiết để sinh ảnh Infographic tổng hợp.
Bản phác thảo cần:
1. Tiêu đề chính thu hút bao quát toàn bộ nội dung.
2. 4-6 ý chính quan trọng nhất từ tất cả các bài học.
3. Mô tả ngắn gọn về phong cách thiết kế (gợi ý: hiện đại, màu sắc hài hòa, bố cục chuyên nghiệp).
4. Chỉ trả về bản tóm tắt tóm gọn nhất trong khoảng 250 từ để làm prompt sinh ảnh.";

        var summary = await _llmService.GenerateResponseAsync(summaryPrompt, combinedContent);

        try
        {
            var imageUrl = await _imageService.GenerateImageAsync(summary);

            var infographic = new LessonInfographic
            {
                ImageUrl = imageUrl,
                Summary = summary,
                CreatedById = adminId,
                Lessons = lessons
            };

            _db.LessonInfographics.Add(infographic);
            await _db.SaveChangesAsync();

            return new AiToolResponse(
                true,
                "Đã tạo Infographic thành công.",
                new { Id = infographic.Id, ImageUrl = imageUrl, Summary = summary, LessonIds = lessonIds });
        } catch(Exception ex)
        {
            return new AiToolResponse(false, $"Lỗi khi sinh ảnh: {ex.Message}");
        }
    }

    private async Task<AiToolResponse> NotifyProgressInternalAsync(string json)
    { return new AiToolResponse(true, "OK"); }
}
