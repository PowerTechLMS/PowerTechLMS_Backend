using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class ProgressService : IProgressService
{
    private readonly AppDbContext _db;
    private readonly ICertificateService _certificateService; // Tiêm vào để tự động cấp khi xong bài học cuối

    public ProgressService(AppDbContext db, ICertificateService certificateService)
    {
        _db = db;
        _certificateService = certificateService;
    }

    public async Task<ProgressResponse> CompleteLessonAsync(int userId, int lessonId)
    {
        var progress = await _db.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress { UserId = userId, LessonId = lessonId };
            _db.LessonProgresses.Add(progress);
        }

        progress.IsCompleted = true;
        progress.CompletedAt = DateTime.UtcNow;
        progress.WatchedPercent = 100;
        await _db.SaveChangesAsync();

        // TỰ ĐỘNG KIỂM TRA HOÀN THÀNH KHÓA HỌC (Trường hợp bài học là nút cuối cùng)
        var lesson = await _db.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson != null)
        {
            var status = await GetCourseProgressAsync(userId, lesson.Module.CourseId);
            if (status.IsCompleted)
            {
                await _certificateService.IssueCertificateAsync(userId, lesson.Module.CourseId);

                var enrollment = await _db.Enrollments
                    .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == lesson.Module.CourseId);
                if (enrollment != null)
                {
                    enrollment.Status = "Completed";
                    await _db.SaveChangesAsync();
                }
            }
        }

        return new ProgressResponse(lessonId, true, progress.PositionSeconds, 100);
    }

    public async Task<CourseProgressResponse> GetCourseProgressAsync(int userId, int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId)
            ?? throw new KeyNotFoundException("Không tìm thấy khóa học.");

        // 1. Chỉ tính % tiến độ dựa trên bài học (Video/Bài đọc)
        var totalLessons = await _db.Lessons.CountAsync(l => l.Module.CourseId == courseId && !l.IsDeleted);
        var completedLessons = await _db.LessonProgresses
            .CountAsync(lp => lp.UserId == userId && lp.IsCompleted && lp.Lesson.Module.CourseId == courseId);

        // 2. Kiểm tra bài thi cuối khóa (Điều kiện bắt buộc để Completed = true)
        var finalQuiz = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted && !_db.Lessons.Any(l => l.QuizId == q.Id))
            .FirstOrDefaultAsync();

        bool quizPassed = false;
        if (finalQuiz == null) quizPassed = true;
        else quizPassed = await _db.QuizAttempts.AnyAsync(qa => qa.UserId == userId && qa.QuizId == finalQuiz.Id && qa.IsPassed);

        // 3. Tính % hiển thị (Dựa 100% vào bài học để học viên không bị báo 50% khi chưa thi)
        double progressPercent = totalLessons > 0 ? (double)completedLessons / totalLessons * 100 : 0;

        // 4. Xác định trạng thái Hoàn thành thực tế
        bool allLessonsDone = totalLessons == 0 || completedLessons >= totalLessons;
        bool isCompleted = allLessonsDone && quizPassed;

        return new CourseProgressResponse(
            courseId, course.Title, Math.Round(progressPercent, 0),
            completedLessons, totalLessons, isCompleted, quizPassed
        );
    }

    // Các hàm UpdateVideoPositionAsync, GetUserProgressAsync... giữ nguyên logic cũ của bạn
    public async Task<ProgressResponse> UpdateVideoPositionAsync(int userId, int lessonId, int positionSeconds, int watchedPercent)
    {
        var progress = await _db.LessonProgresses.FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);
        if (progress == null) { progress = new LessonProgress { UserId = userId, LessonId = lessonId }; _db.LessonProgresses.Add(progress); }
        progress.PositionSeconds = positionSeconds;
        progress.WatchedPercent = Math.Max(progress.WatchedPercent, watchedPercent);
        await _db.SaveChangesAsync();
        return new ProgressResponse(lessonId, progress.IsCompleted, positionSeconds, progress.WatchedPercent);
    }

    public async Task<List<CourseProgressResponse>> GetUserProgressAsync(int userId)
    {
        var enrollments = await _db.Enrollments.Where(e => e.UserId == userId && e.Status == "Approved").Select(e => e.CourseId).ToListAsync();
        var result = new List<CourseProgressResponse>();
        foreach (var cId in enrollments) result.Add(await GetCourseProgressAsync(userId, cId));
        return result;
    }

    public async Task<List<ProgressResponse>> GetLessonProgressesAsync(int userId, int courseId)
    {
        return await _db.LessonProgresses.Where(lp => lp.UserId == userId && lp.Lesson.Module.CourseId == courseId)
            .Select(lp => new ProgressResponse(lp.LessonId, lp.IsCompleted, lp.PositionSeconds, lp.WatchedPercent)).ToListAsync();
    }

    public async Task<bool> CanAccessLessonAsync(int userId, int lessonId)
    {
        var lesson = await _db.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null || lesson.IsFreePreview) return true;
        var user = await _db.Users.FindAsync(userId);
        if (user?.Role == "Admin") return true;
        var enrollment = await _db.Enrollments.AnyAsync(e => e.UserId == userId && e.CourseId == lesson.Module.CourseId && (e.Status == "Approved" || e.Status == "Completed"));
        if (!enrollment) return false;
        var allLessons = await _db.Lessons.Where(l => l.Module.CourseId == lesson.Module.CourseId).OrderBy(l => l.Module.SortOrder).ThenBy(l => l.SortOrder).ToListAsync();
        var idx = allLessons.FindIndex(l => l.Id == lessonId);
        if (idx == 0) return true;
        return await _db.LessonProgresses.AnyAsync(lp => lp.UserId == userId && lp.LessonId == allLessons[idx - 1].Id && lp.IsCompleted);
    }
}