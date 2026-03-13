using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;


public class CertificateService : ICertificateService
{
    private readonly AppDbContext _db;

    // FIX: Loại bỏ IProgressService khỏi constructor để phá vỡ vòng lặp dependency
    public CertificateService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CertificateResponse?> IssueCertificateAsync(int userId, int courseId)
    {
        // 1. Kiểm tra xem đã tồn tại chứng chỉ chưa
        var existing = await _db.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.UserId == userId && c.CourseId == courseId);

        if (existing != null)
        {
            return new CertificateResponse(
                existing.Id,
                existing.User?.FullName ?? "Ẩn danh",
                existing.Course?.Title ?? "Khóa học",
                existing.CertificateCode ?? "",
                existing.PdfUrl,
                existing.IssuedAt,
                existing.Status ?? "Issued",
                existing.RevokedAt);
        }

        // 2. TỰ KIỂM TRA ĐIỀU KIỆN HOÀN THÀNH (Thay thế cho ProgressService)
        // Đếm tổng số bài học
        var totalLessons = await _db.Lessons
            .CountAsync(l => l.Module.CourseId == courseId && !l.IsDeleted);

        // Đếm số bài học đã hoàn thành
        var completedLessons = await _db.LessonProgresses
            .CountAsync(lp => lp.UserId == userId && lp.IsCompleted && lp.Lesson.Module.CourseId == courseId);

        // Kiểm tra bài thi cuối khóa (Final Quiz)
        var finalQuiz = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .FirstOrDefaultAsync();

        bool quizPassed = true;
        if (finalQuiz != null)
        {
            quizPassed = await _db.QuizAttempts
                .AnyAsync(qa => qa.UserId == userId && qa.QuizId == finalQuiz.Id && qa.IsPassed);
        }

        // Kiểm tra logic hoàn thành: Phải xong hết video và đạt bài thi (nếu có)
        bool isCompleted = (totalLessons == 0 || completedLessons >= totalLessons) && quizPassed;

        if (!isCompleted)
            throw new InvalidOperationException("Bạn chưa đủ điều kiện nhận chứng chỉ. Cần hoàn thành 100% nội dung và đạt bài thi.");

        // 3. TẠO MÃ CHỨNG CHỈ
        var certCode = $"CERT-{userId}-{courseId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

        var cert = new Certificate
        {
            UserId = userId,
            CourseId = courseId,
            CertificateCode = certCode,
            Status = "Issued",
            IssuedAt = DateTime.UtcNow
        };

        _db.Certificates.Add(cert);

        // 4. ĐỒNG BỘ STATUS TRONG ENROLLMENT
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);
        if (enrollment != null)
        {
            enrollment.Status = "Completed";
        }

        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        var course = await _db.Courses.FindAsync(courseId);

        return new CertificateResponse(
            cert.Id,
            user?.FullName ?? "Ẩn danh",
            course?.Title ?? "Khóa học",
            cert.CertificateCode,
            cert.PdfUrl,
            cert.IssuedAt,
            cert.Status,
            cert.RevokedAt);
    }

    public async Task<List<CertificateResponse>> GetUserCertificatesAsync(int userId)
    {
        // Đảm bảo lấy được mọi chứng chỉ cũ/mới của User mà không bị lọc sai
        return await _db.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new CertificateResponse(
                c.Id,
                c.User != null ? c.User.FullName : "Học viên",
                c.Course != null ? c.Course.Title : "Khóa học đã xóa",
                c.CertificateCode ?? "Chưa có mã",
                c.PdfUrl,
                c.IssuedAt,
                c.Status ?? "Issued",
                c.RevokedAt))
            .ToListAsync();
    }

    public async Task<CertificateResponse?> VerifyCertificateAsync(string code)
    {
        return await _db.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .Where(c => c.CertificateCode == code)
            .Select(c => new CertificateResponse(
                c.Id,
                c.User != null ? c.User.FullName : "Ẩn danh",
                c.Course != null ? c.Course.Title : "Khóa học",
                c.CertificateCode ?? "",
                c.PdfUrl,
                c.IssuedAt,
                c.Status ?? "Issued",
                c.RevokedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<PagedResponse<AdminCertificateResponse>> GetCertificatesAsync(int page, int pageSize, string? search)
    {
        var query = _db.Certificates
            .Include(c => c.User)
            .Include(c => c.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c =>
                (c.User != null && c.User.FullName.ToLower().Contains(s)) ||
                (c.CertificateCode.ToLower().Contains(s)) ||
                (c.Course != null && c.Course.Title.ToLower().Contains(s))
            );
        }

        var total = await query.CountAsync();
        var rawItems = await query
            .OrderByDescending(c => c.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = rawItems.Select(c => new AdminCertificateResponse(
            c.Id,
            c.User?.FullName ?? "Học viên ẩn danh",
            c.CertificateCode ?? "N/A",
            c.Course?.Title ?? "Khóa học",
            100.0,
            c.IssuedAt,
            c.Status ?? "Issued",
            c.RevokedAt
        )).ToList();

        return new PagedResponse<AdminCertificateResponse>(items, total, page, pageSize);
    }

    public async Task RevokeCertificateAsync(int id, string reason, int adminId)
    {
        var cert = await _db.Certificates.FindAsync(id);
        if (cert == null) throw new KeyNotFoundException("Không tìm thấy chứng chỉ.");
        cert.Status = "Revoked";
        cert.RevokedAt = DateTime.UtcNow;
        cert.RevokeReason = reason;
        await _db.SaveChangesAsync();
    }
}

public class QAService : IQAService
{
    private readonly AppDbContext _db;
    public QAService(AppDbContext db) => _db = db;

    public async Task<QAResponse> CreatePostAsync(int lessonId, int userId, CreateQARequest request)
    {
        var post = new QAThread
        {
            LessonId = lessonId,
            UserId = userId,
            Content = request.Content,
            ParentId = request.ParentId
        };
        _db.QAThreads.Add(post);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        return new QAResponse(post.Id, post.Content, user!.FullName, user.Avatar, post.ParentId, post.CreatedAt, new List<QAResponse>());
    }

    public async Task<List<QAResponse>> GetLessonQAAsync(int lessonId)
    {
        var posts = await _db.QAThreads
            .Include(qa => qa.User)
            .Include(qa => qa.Replies).ThenInclude(r => r.User)
            .Where(qa => qa.LessonId == lessonId && qa.ParentId == null)
            .OrderByDescending(qa => qa.CreatedAt)
            .ToListAsync();

        return posts.Select(MapQA).ToList();
    }

    private QAResponse MapQA(QAThread qa)
    {
        return new QAResponse(qa.Id, qa.Content, qa.User.FullName, qa.User.Avatar,
            qa.ParentId, qa.CreatedAt,
            qa.Replies.OrderBy(r => r.CreatedAt).Select(MapQA).ToList());
    }

    public async Task DeletePostAsync(int postId, int userId)
    {
        var post = await _db.QAThreads.FindAsync(postId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài viết.");
        if (post.UserId != userId) throw new UnauthorizedAccessException("Không có quyền xóa.");
        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}

public class NoteService : INoteService
{
    private readonly AppDbContext _db;
    public NoteService(AppDbContext db) => _db = db;

    public async Task<NoteResponse> CreateNoteAsync(int lessonId, int userId, CreateNoteRequest request)
    {
        var note = new Note
        {
            LessonId = lessonId,
            UserId = userId,
            Content = request.Content,
            VideoTimestampSeconds = request.VideoTimestampSeconds
        };
        _db.Notes.Add(note);
        await _db.SaveChangesAsync();
        return new NoteResponse(note.Id, note.Content, note.VideoTimestampSeconds, note.CreatedAt);
    }

    public async Task<List<NoteResponse>> GetLessonNotesAsync(int lessonId, int userId)
    {
        return await _db.Notes
            .Where(n => n.LessonId == lessonId && n.UserId == userId)
            .OrderBy(n => n.VideoTimestampSeconds)
            .Select(n => new NoteResponse(n.Id, n.Content, n.VideoTimestampSeconds, n.CreatedAt))
            .ToListAsync();
    }

    public async Task DeleteNoteAsync(int noteId, int userId)
    {
        var note = await _db.Notes.FindAsync(noteId)
            ?? throw new KeyNotFoundException("Không tìm thấy ghi chú.");
        if (note.UserId != userId) throw new UnauthorizedAccessException("Không có quyền xóa.");
        note.IsDeleted = true;
        note.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _db;
    public LeaderboardService(AppDbContext db) => _db = db;

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(int top = 10)
    {
        var users = await _db.Users
            .Where(u => u.Role == "Employee")
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.Avatar,
                CompletedCourses = _db.Certificates.Count(c => c.UserId == u.Id),
                Badges = _db.UserBadges.Where(ub => ub.UserId == u.Id)
                    .Select(ub => new BadgeResponse(ub.Badge.Id, ub.Badge.Name, ub.Badge.Description,
                        ub.Badge.IconUrl, ub.Badge.RequiredCourses, true, ub.EarnedAt))
                    .ToList()
            })
            .OrderByDescending(u => u.CompletedCourses)
            .Take(top)
            .ToListAsync();

        return users.Select((u, i) => new LeaderboardEntry(
            i + 1, u.Id, u.FullName, u.Avatar, u.CompletedCourses,
            u.CompletedCourses * 100, u.Badges)).ToList();
    }

    // 1. TOP 10 NHÂN VIÊN CHĂM CHỈ NHẤT THÁNG
    public async Task<List<LeaderboardResponse>> GetMonthlyLeaderboardAsync()
    {
        var currentMonth = DateTime.UtcNow.Month;
        var currentYear = DateTime.UtcNow.Year;

        // Đếm số bài học hoàn thành trong tháng này của từng user
        var topUsers = await _db.LessonProgresses
            .Where(lp => lp.IsCompleted && lp.CompletedAt.HasValue
                      && lp.CompletedAt.Value.Month == currentMonth
                      && lp.CompletedAt.Value.Year == currentYear
                      && !lp.IsDeleted)
            .GroupBy(lp => lp.UserId)
            .Select(g => new { UserId = g.Key, CompletedCount = g.Count() })
            .OrderByDescending(x => x.CompletedCount)
            .Take(10)
            .ToListAsync();

        var result = new List<LeaderboardResponse>();
        int rank = 1;

        foreach (var item in topUsers)
        {
            var user = await _db.Users.FindAsync(item.UserId);
            if (user != null)
            {
                result.Add(new LeaderboardResponse(rank++, user.Id, user.FullName, user.Avatar, item.CompletedCount));
            }
        }
        return result;
    }

    // 2. LẤY DANH SÁCH HUY HIỆU CỦA USER
    public async Task<List<UserBadgeResponse>> GetUserBadgesAsync(int userId)
    {
        return await _db.UserBadges
            .Include(ub => ub.Badge)
            .Where(ub => ub.UserId == userId && !ub.IsDeleted)
            .OrderByDescending(ub => ub.EarnedAt)
            .Select(ub => new UserBadgeResponse(ub.BadgeId, ub.Badge.Name, ub.Badge.Description, ub.Badge.IconUrl, ub.EarnedAt))
            .ToListAsync();
    }

    // 3. LOGIC TỰ ĐỘNG CẤP HUY HIỆU (Gọi hàm này mỗi khi user hoàn thành 1 khóa học)
    public async Task CheckAndAwardBadgesAsync(int userId)
    {
        var completedCoursesCount = await _db.Enrollments
            .CountAsync(e => e.UserId == userId && e.Status == "Completed" && !e.IsDeleted);

        // Lấy các huy hiệu đủ điều kiện nhưng user chưa có
        var eligibleBadges = await _db.Badges
            .Where(b => b.RequiredCourses <= completedCoursesCount && !b.IsDeleted)
            .Where(b => !_db.UserBadges.Any(ub => ub.BadgeId == b.Id && ub.UserId == userId))
            .ToListAsync();

        if (eligibleBadges.Any())
        {
            foreach (var badge in eligibleBadges)
            {
                _db.UserBadges.Add(new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id,
                    EarnedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();
        }
    }
}

public class DocumentService : IDocumentService
{
    private readonly AppDbContext _db;
    public DocumentService(AppDbContext db) => _db = db;

    public async Task<PagedResponse<DocumentResponse>> GetDocumentsAsync(int page, int pageSize, string? search, string? tag, bool isAdmin = false, int? userId = null)
    {
        // Bắt đầu query cơ bản
        var query = _db.Documents
            .Include(d => d.UploadedBy)
            .Include(d => d.CurrentVersion)
            .AsQueryable();

        // 1. TÌM KIẾM CƠ BẢN & THEO TAG
        if (!string.IsNullOrWhiteSpace(search))
        {
            // (Tính năng Full-text search PDF sẽ được bổ sung sau nếu bạn tích hợp thư viện)
            query = query.Where(d => d.Title.Contains(search) || d.Description.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            // Lọc những tài liệu có chứa Tag này
            query = query.Where(d => d.Tags.Contains(tag));
        }

        // 2. LOGIC PHÂN QUYỀN (Nếu KHÔNG phải Admin)
        if (!isAdmin && userId.HasValue)
        {
            var now = DateTime.UtcNow;

            // Lấy danh sách Role và Group của User hiện tại
            var userRoles = await _db.UserRoles.Where(ur => ur.UserId == userId).Select(ur => ur.RoleId).ToListAsync();
            var userGroups = await _db.UserGroupMembers.Where(ugm => ugm.UserId == userId).Select(ugm => ugm.GroupId).ToListAsync();

            query = query.Where(d =>
                // Điều kiện 1: Vẫn còn hạn truy cập
                (d.AccessStartDate == null || d.AccessStartDate <= now) &&
                (d.AccessEndDate == null || d.AccessEndDate >= now) &&

                // Điều kiện 2: Được cấp quyền (hoặc là tài liệu public không có dòng permission nào)
                (!d.Permissions.Any() || d.Permissions.Any(p =>
                    p.UserId == userId ||
                    (p.RoleId.HasValue && userRoles.Contains(p.RoleId.Value)) ||
                    (p.UserGroupId.HasValue && userGroups.Contains(p.UserGroupId.Value))
                ))
            );
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DocumentResponse(
                d.Id, d.Title, d.Description,
                d.CurrentVersion != null ? d.CurrentVersion.FileName : "",
                d.CurrentVersion != null ? d.CurrentVersion.FileSize : 0,
                d.Tags, d.UploadedBy.FullName, d.CreatedAt,
                d.CurrentVersion != null ? d.CurrentVersion.VersionNumber : 1,
                d.AccessStartDate, d.AccessEndDate))
            .ToListAsync();

        return new PagedResponse<DocumentResponse>(items, total, page, pageSize);
    }

    public async Task<DocumentResponse> CreateDocumentAsync(CreateDocumentRequest request, int userId, Stream fileStream, string fileName, long fileSize)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(fileName);
        var storageKey = $"documents/{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, Path.GetFileName(storageKey));

        using (var fs = new FileStream(filePath, FileMode.Create))
            await fileStream.CopyToAsync(fs);

        var doc = new Document
        {
            Title = request.Title,
            Description = request.Description,
            Tags = request.Tags,
            UploadedById = userId,
            AccessStartDate = request.AccessStartDate,
            AccessEndDate = request.AccessEndDate
        };
        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        var version = new DocumentVersion
        {
            DocumentId = doc.Id,
            VersionNumber = 1,
            FileName = fileName,
            StorageKey = storageKey,
            FileSize = fileSize,
            FileType = ext.TrimStart('.'),
            ChangeNote = "Phiên bản đầu tiên",
            UploadedById = userId
        };
        _db.DocumentVersions.Add(version);
        await _db.SaveChangesAsync();

        doc.CurrentVersionId = version.Id;
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        return new DocumentResponse(doc.Id, doc.Title, doc.Description,
            version.FileName, version.FileSize, doc.Tags, user!.FullName, doc.CreatedAt,
            1, doc.AccessStartDate, doc.AccessEndDate);
    }

    public async Task<DocumentResponse> UpdateDocumentAsync(int documentId, UpdateDocumentRequest request)
    {
        var doc = await _db.Documents.Include(d => d.UploadedBy).Include(d => d.CurrentVersion)
            .FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");

        doc.Title = request.Title;
        doc.Description = request.Description;
        doc.Tags = request.Tags;
        doc.AccessStartDate = request.AccessStartDate;
        doc.AccessEndDate = request.AccessEndDate;
        doc.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new DocumentResponse(doc.Id, doc.Title, doc.Description,
            doc.CurrentVersion?.FileName ?? "", doc.CurrentVersion?.FileSize ?? 0,
            doc.Tags, doc.UploadedBy.FullName, doc.CreatedAt,
            doc.CurrentVersion?.VersionNumber ?? 1,
            doc.AccessStartDate, doc.AccessEndDate);
    }

    public async Task<DocumentResponse> AddVersionAsync(int documentId, int userId, Stream fileStream, string fileName, long fileSize, string? changeNote)
    {
        var doc = await _db.Documents.Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
        var ext = Path.GetExtension(fileName);
        var storageKey = $"documents/{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, Path.GetFileName(storageKey));
        using (var fs = new FileStream(filePath, FileMode.Create))
            await fileStream.CopyToAsync(fs);

        var nextVersion = (doc.Versions.Any() ? doc.Versions.Max(v => v.VersionNumber) : 0) + 1;
        var version = new DocumentVersion
        {
            DocumentId = documentId,
            VersionNumber = nextVersion,
            FileName = fileName,
            StorageKey = storageKey,
            FileSize = fileSize,
            FileType = ext.TrimStart('.'),
            ChangeNote = changeNote,
            UploadedById = userId
        };
        _db.DocumentVersions.Add(version);
        await _db.SaveChangesAsync();

        doc.CurrentVersionId = version.Id;
        doc.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);
        return new DocumentResponse(doc.Id, doc.Title, doc.Description,
            version.FileName, version.FileSize, doc.Tags, user!.FullName, doc.CreatedAt,
            nextVersion, doc.AccessStartDate, doc.AccessEndDate);
    }

    public async Task<List<DocumentVersionResponse>> GetVersionsAsync(int documentId)
    {
        return await _db.DocumentVersions
            .Include(v => v.UploadedBy)
            .Where(v => v.DocumentId == documentId && !v.IsDeleted)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionResponse(
                v.Id, v.DocumentId, v.VersionNumber, v.FileName, v.FileSize,
                v.FileType, v.ChangeNote, v.UploadedBy.FullName, v.CreatedAt))
            .ToListAsync();
    }

    public async Task<(Stream stream, string fileName, string contentType)> GetVersionFileAsync(int versionId)
    {
        var version = await _db.DocumentVersions.FindAsync(versionId)
            ?? throw new KeyNotFoundException("Không tìm thấy phiên bản.");
        var storageKey = version.StorageKey ?? string.Empty;
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents", Path.GetFileName(storageKey));
        if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException("Tệp vật lý không tồn tại.");
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var contentType = version.FileType switch
        {
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
        return (stream, version.FileName, contentType);
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        var doc = await _db.Documents.FindAsync(documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");
        doc.IsDeleted = true;
        doc.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<(Stream stream, string fileName, string contentType)> GetFileAsync(int documentId)
    {
        var doc = await _db.Documents.Include(d => d.CurrentVersion)
            .FirstOrDefaultAsync(d => d.Id == documentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu.");
        if (doc.CurrentVersion == null) throw new InvalidOperationException("Tài liệu không có nội dung.");

        var storageKey = doc.CurrentVersion.StorageKey ?? string.Empty;
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "documents", Path.GetFileName(storageKey));
        if (!System.IO.File.Exists(filePath)) throw new FileNotFoundException("Tệp vật lý không tồn tại.");

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var contentType = doc.CurrentVersion.FileType switch
        {
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            _ => "application/octet-stream"
        };
        return (stream, doc.CurrentVersion.FileName, contentType);
    }
    public async Task<DocumentConfigResponse?> GetDocumentConfigAsync(int documentId)
    {
        var doc = await _db.Documents.FirstOrDefaultAsync(d => d.Id == documentId);
        if (doc == null) return null;

        return new DocumentConfigResponse(
            doc.Id, doc.Title, doc.Description, doc.Tags,
            doc.AccessStartDate, doc.AccessEndDate
        );
    }

    public async Task<List<DocumentPermissionResponse>> GetDocumentPermissionsAsync(int documentId)
    {
        return await _db.DocumentPermissions
            .Where(p => p.DocumentId == documentId && !p.IsDeleted)
            .Select(p => new DocumentPermissionResponse(p.Id, p.DocumentId, p.RoleId, p.UserGroupId, p.UserId))
            .ToListAsync();
    }

    public async Task UpdateDocumentPermissionsAsync(int documentId, UpdatePermissionRequest req)
    {
        // 1. Xóa quyền cũ
        var oldPerms = _db.DocumentPermissions.Where(p => p.DocumentId == documentId);
        _db.DocumentPermissions.RemoveRange(oldPerms);

        // 2. Thêm quyền mới
        if (req.RoleIds != null)
            foreach (var roleId in req.RoleIds) _db.DocumentPermissions.Add(new DocumentPermission { DocumentId = documentId, RoleId = roleId });

        if (req.GroupIds != null)
            foreach (var groupId in req.GroupIds) _db.DocumentPermissions.Add(new DocumentPermission { DocumentId = documentId, UserGroupId = groupId });

        if (req.UserIds != null)
            foreach (var userId in req.UserIds) _db.DocumentPermissions.Add(new DocumentPermission { DocumentId = documentId, UserId = userId });

        await _db.SaveChangesAsync();
    }

    public async Task ClearDocumentPermissionsAsync(int documentId)
    {
        var oldPerms = _db.DocumentPermissions.Where(p => p.DocumentId == documentId);
        _db.DocumentPermissions.RemoveRange(oldPerms);
        await _db.SaveChangesAsync();
    }
}

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    public ReportService(AppDbContext db) => _db = db;

    public async Task<List<TrainingReportResponse>> GetTrainingReportAsync(int? courseId)
    {
        var query = _db.Enrollments
            .Include(e => e.User).Include(e => e.Course)
            .Where(e => e.Status == "Approved")
            .AsQueryable();

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        var enrollments = await query.ToListAsync();
        var result = new List<TrainingReportResponse>();

        foreach (var e in enrollments)
        {
            var totalLessons = await _db.Lessons.CountAsync(l => l.Module.CourseId == e.CourseId);
            var completedLessons = await _db.LessonProgresses
                .CountAsync(lp => lp.UserId == e.UserId && lp.IsCompleted && lp.Lesson.Module.CourseId == e.CourseId);

            var progress = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;
            var isOverdue = e.Deadline.HasValue && e.Deadline.Value < DateTime.UtcNow && progress < 100;
            var status = progress >= 100 ? "Hoàn thành" : progress > 0 ? "Đang học" : "Chưa bắt đầu";

            result.Add(new TrainingReportResponse(e.UserId, e.User.FullName, e.User.Email,
                e.CourseId, e.Course.Title, Math.Round(progress, 1), status, e.Deadline, isOverdue, e.EnrolledAt));
        }

        return result;
    }

    // 1. BÁO CÁO LƯỜI HỌC (Đã đăng ký nhưng 30 ngày chưa có hoạt động)
    public async Task<List<InactiveUserResponse>> GetInactiveUsersAsync(int days = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var result = new List<InactiveUserResponse>();

        // Lấy các ghi danh đang được yêu cầu học
        var activeEnrollments = await _db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.Status == "Approved" && !e.IsDeleted)
            .ToListAsync();

        foreach (var e in activeEnrollments)
        {
            // Tìm ngày hoạt động gần nhất của user trong khóa học này
            var lastActivity = await _db.LessonProgresses
                .Where(lp => lp.UserId == e.UserId && lp.Lesson.Module.CourseId == e.CourseId)
                .OrderByDescending(lp => lp.UpdatedAt)
                .Select(lp => (DateTime?)lp.UpdatedAt)
                .FirstOrDefaultAsync();

            // Nếu chưa học bài nào, lấy ngày ghi danh làm mốc
            DateTime effectiveLastActive = lastActivity ?? e.EnrolledAt;

            // Nếu mốc thời gian đó cũ hơn cutoffDate -> Đưa vào sổ đen
            if (effectiveLastActive < cutoffDate)
            {
                var inactiveDays = (int)(DateTime.UtcNow - effectiveLastActive).TotalDays;
                result.Add(new InactiveUserResponse(
                    e.UserId, e.User.FullName, e.User.Email,
                    e.CourseId, e.Course.Title, effectiveLastActive, inactiveDays
                ));
            }
        }

        // Sắp xếp người lười nhất lên đầu (số ngày không hoạt động cao nhất)
        return result.OrderByDescending(x => x.InactiveDays).ToList();
    }

    // 2. PHÂN TÍCH ĐỀ THI (Câu nào sai nhiều nhất)
    public async Task<List<QuizAnalyticsResponse>> GetQuizAnalyticsAsync(int quizId)
    {
        // Lấy lịch sử chọn đáp án của toàn bộ học viên cho bài Quiz này
        // Tối ưu truy vấn bằng GroupBy thẳng dưới SQL Server
        var analytics = await _db.QuizAnswers
            .Include(qa => qa.Attempt)
            .Where(qa => qa.Attempt.QuizId == quizId && !qa.IsDeleted)
           .GroupBy(qa => qa.QuestionId) // Sửa lại thành như thế này
            .Select(g => new
            {
                QuestionId = g.Key,
                TotalAttempts = g.Count(),
                WrongAnswers = g.Count(qa => qa.IsCorrect == false)
            })
            .ToListAsync();

        var response = new List<QuizAnalyticsResponse>();

        foreach (var item in analytics)
        {
            var question = await _db.QuestionBanks.FirstOrDefaultAsync(q => q.Id == item.QuestionId);
            if (question == null) continue;

            double errorRate = item.TotalAttempts > 0
                ? Math.Round((double)item.WrongAnswers / item.TotalAttempts * 100, 2)
                : 0;

            response.Add(new QuizAnalyticsResponse(
                question.Id, question.QuestionText,
                item.TotalAttempts, item.WrongAnswers, errorRate
            ));
        }

        // Trả về danh sách xếp theo tỷ lệ sai cao nhất xếp trước
        return response.OrderByDescending(x => x.ErrorRate).ToList();
    }
}
