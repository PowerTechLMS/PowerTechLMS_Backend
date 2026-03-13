using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class ModuleService : IModuleService
{
    private readonly AppDbContext _db;
    public ModuleService(AppDbContext db) => _db = db;

    public async Task<ModuleResponse> CreateModuleAsync(int courseId, CreateModuleRequest request)
    {
        var module = new Module { CourseId = courseId, Title = request.Title, SortOrder = request.SortOrder };
        _db.Modules.Add(module);
        await _db.SaveChangesAsync();
        return new ModuleResponse(module.Id, module.Title, module.SortOrder, new List<LessonResponse>());
    }


    public async Task<ModuleResponse> UpdateModuleAsync(int moduleId, UpdateModuleRequest request)
    {
        var module = await _db.Modules.Include(m => m.Lessons.OrderBy(l => l.SortOrder))
            .ThenInclude(l => l.Attachments)
            .FirstOrDefaultAsync(m => m.Id == moduleId)
            ?? throw new KeyNotFoundException("Không tìm thấy chương.");

        module.Title = request.Title;
        module.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync();

        return new ModuleResponse(module.Id, module.Title, module.SortOrder,
            module.Lessons.Select(l => new LessonResponse(l.Id, l.Title, l.Type, l.Content, l.VideoStorageUrl ?? l.VideoUrl,
                l.VideoDurationSeconds, l.SortOrder, l.IsFreePreview,
                l.Attachments.Select(a => new AttachmentResponse(a.Id, a.FileName, a.FileSize)).ToList()
            )).ToList());
    }

    public async Task DeleteModuleAsync(int moduleId)
    {
        var module = await _db.Modules.FindAsync(moduleId)
            ?? throw new KeyNotFoundException("Không tìm thấy chương.");
        module.IsDeleted = true;
        module.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(List<SortOrderItem> items)
    {
        foreach (var item in items)
        {
            var module = await _db.Modules.FindAsync(item.Id);
            if (module != null) module.SortOrder = item.SortOrder;
        }
        await _db.SaveChangesAsync();
    }
}

public class LessonService : ILessonService
{
    private readonly AppDbContext _db;

    public LessonService(AppDbContext db) => _db = db;
    public async Task<(Stream stream, string fileName, string contentType)> GetAttachmentFileAsync(int attachmentId)
    {
        // 1. Tìm file trong Database
        var attachment = await _db.LessonAttachments.FindAsync(attachmentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tài liệu đính kèm.");

        // 2. Lấy đường dẫn vật lý (Dựa trên cột StorageKey Backend đã lưu lúc Upload)
        var storageKey = attachment.StorageKey ?? string.Empty;
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", storageKey.TrimStart('/'));
        if (!System.IO.File.Exists(filePath))
            throw new FileNotFoundException("Tệp vật lý không tồn tại trên máy chủ.");

        // 3. Đọc file
        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        // 4. Xác định định dạng file để trình duyệt tải đúng
        var ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };

        return (stream, attachment.FileName, contentType);
    }

    public async Task<LessonResponse> CreateLessonAsync(int moduleId, CreateLessonRequest request)
    {
        var lesson = new Lesson
        {
            ModuleId = moduleId,
            Title = request.Title,
            Type = request.Type,
            Content = request.Content,
            VideoUrl = request.VideoUrl,
            SortOrder = request.SortOrder,
            IsFreePreview = request.IsFreePreview
        };
        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync();
        return new LessonResponse(lesson.Id, lesson.Title, lesson.Type, lesson.Content, lesson.VideoStorageUrl ?? lesson.VideoUrl,
            lesson.VideoDurationSeconds, lesson.SortOrder, lesson.IsFreePreview, new List<AttachmentResponse>());
    }

    public async Task<int> CreateLessonQuizAsync(int lessonId, CreateQuizRequest request)
    {
        // 1. Lấy Bài học KÈM THEO Module để biết CourseId (Tránh lỗi SQL thiếu CourseId)
        var lesson = await _db.Lessons
            .Include(l => l.Module)
            .FirstOrDefaultAsync(l => l.Id == lessonId);

        if (lesson == null) throw new KeyNotFoundException("Không tìm thấy bài học.");

        // 2. TẠO THÔNG TIN CHUNG CỦA QUIZ (Không chứa câu hỏi)
        var newQuiz = new Quiz
        {
            Title = string.IsNullOrWhiteSpace(request.Title) ? "Bài tập củng cố" : request.Title,
            TimeLimitMinutes = request.TimeLimitMinutes ?? 0,
            PassScore = request.PassScore,
            QuestionCount = request.QuestionCount,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleAnswers = request.ShuffleAnswers,
            CourseId = lesson.Module.CourseId, // Bắt buộc phải có để SQL không báo lỗi
            CreatedAt = DateTime.UtcNow
        };

        _db.Quizzes.Add(newQuiz);
        await _db.SaveChangesAsync();

        // 3. Gắn QuizId vừa tạo vào Bài học
        lesson.QuizId = newQuiz.Id;
        await _db.SaveChangesAsync();

        // Trả về ID để Frontend tiếp tục gọi API AddQuestion
        return newQuiz.Id;
    }
    public async Task<LessonResponse> UpdateLessonAsync(int lessonId, UpdateLessonRequest request)
    {
        var lesson = await _db.Lessons.Include(l => l.Attachments).FirstOrDefaultAsync(l => l.Id == lessonId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài học.");

        lesson.Title = request.Title;
        lesson.Type = request.Type;
        lesson.Content = request.Content;
        lesson.VideoUrl = request.VideoUrl;
        lesson.SortOrder = request.SortOrder;
        lesson.IsFreePreview = request.IsFreePreview;
        lesson.VideoDurationSeconds = request.VideoDurationSeconds;
        await _db.SaveChangesAsync();

        return new LessonResponse(lesson.Id, lesson.Title, lesson.Type, lesson.Content, lesson.VideoStorageUrl ?? lesson.VideoUrl,
            lesson.VideoDurationSeconds, lesson.SortOrder, lesson.IsFreePreview,
            lesson.Attachments.Select(a => new AttachmentResponse(a.Id, a.FileName, a.FileSize)).ToList());
    }

    public async Task DeleteLessonAsync(int lessonId)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài học.");
        lesson.IsDeleted = true;
        lesson.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateSortOrderAsync(List<SortOrderItem> items)
    {
        foreach (var item in items)
        {
            var lesson = await _db.Lessons.FindAsync(item.Id);
            if (lesson != null) lesson.SortOrder = item.SortOrder;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<AttachmentResponse> UploadAttachmentAsync(int lessonId, Stream fileStream, string fileName, long fileSize)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "attachments");
        if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(fileName);
        var newFileName = $"{lessonId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsDir, newFileName);

        using (var fs = new FileStream(filePath, FileMode.Create))
            await fileStream.CopyToAsync(fs);

        var attachment = new LessonAttachment
        {
            LessonId = lessonId,
            FileName = fileName,
            StorageKey = $"attachments/{newFileName}",
            FileSize = fileSize
        };
        _db.LessonAttachments.Add(attachment);
        await _db.SaveChangesAsync();
        return new AttachmentResponse(attachment.Id, attachment.FileName, attachment.FileSize);
    }

    public async Task<string> UploadVideoAsync(int lessonId, Stream fileStream, string fileName)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài học.");

        var videosDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "videos");
        if (!Directory.Exists(videosDir)) Directory.CreateDirectory(videosDir);

        var ext = Path.GetExtension(fileName);
        var newFileName = $"video_{lessonId}_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(videosDir, newFileName);

        using (var fs = new FileStream(filePath, FileMode.Create))
            await fileStream.CopyToAsync(fs);

        lesson.VideoProvider = "Local";
        lesson.VideoStorageKey = $"videos/{newFileName}";
        lesson.VideoStorageUrl = $"/uploads/videos/{newFileName}";
        // Clear old youtube URL if any
        lesson.VideoUrl = null; 

        await _db.SaveChangesAsync();
        return lesson.VideoStorageUrl;
    }

    public async Task DeleteAttachmentAsync(int attachmentId)
    {
        var attachment = await _db.LessonAttachments.FindAsync(attachmentId)
            ?? throw new KeyNotFoundException("Không tìm thấy tệp đính kèm.");
        attachment.IsDeleted = true;
        attachment.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

}
