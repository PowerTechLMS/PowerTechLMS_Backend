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

    public async Task<ProgressResponse> CompleteLessonAsync(int userId, int lessonId, bool isQuizPassed = false)
    {
        var lesson = await _db.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId)
            ?? throw new KeyNotFoundException("Không tìm thấy bài học.");

        // [MỚI] KIỂM TRA QUIZ NẾU CÓ: Nếu bài học có Quiz, phải Pass quiz mới được Complete Lesson
        bool actuallyPassed = isQuizPassed;
        if (lesson.QuizId.HasValue && !actuallyPassed)
        {
            actuallyPassed = await _db.QuizAttempts.AnyAsync(qa => qa.UserId == userId && qa.QuizId == lesson.QuizId.Value && qa.IsPassed);
            
            if (!actuallyPassed)
            {
                // Nếu video đã xem đủ (watchedPercent cao) nhưng chưa xong quiz, ta vẫn lưu WatchedPercent nhưng IsCompleted = false
                var current = await _db.LessonProgresses.FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);
                if (current == null) {
                    current = new LessonProgress { UserId = userId, LessonId = lessonId, WatchedPercent = 100, PositionSeconds = lesson.VideoDurationSeconds };
                    _db.LessonProgresses.Add(current);
                } else {
                    current.WatchedPercent = 100;
                    current.PositionSeconds = lesson.VideoDurationSeconds;
                }
                await _db.SaveChangesAsync();
                
                return new ProgressResponse(lessonId, false, current.PositionSeconds, 100, false, true);
            }
        }

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

        // TỰ ĐỘNG KIỂM TRA HOÀN THÀNH KHÓA HỌC
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

        return new ProgressResponse(lessonId, true, progress.PositionSeconds, 100, actuallyPassed, lesson.QuizId.HasValue);
    }

    public async Task<CourseProgressResponse> GetCourseProgressAsync(int userId, int courseId)
    {
        var course = await _db.Courses.FindAsync(courseId);
        if (course == null) return null;

        // 1. Chỉ tính % tiến độ dựa trên bài học (Video/Bài đọc)
        var lessonIds = await _db.Modules
            .Where(m => m.CourseId == courseId && !m.IsDeleted)
            .SelectMany(m => m.Lessons)
            .Where(l => !l.IsDeleted)
            .Select(l => l.Id)
            .ToListAsync();

        var totalLessons = lessonIds.Count;
        var completedLessons = await _db.LessonProgresses
            .CountAsync(lp => lp.UserId == userId && lp.IsCompleted && lessonIds.Contains(lp.LessonId));

        // 2. LẤY TẤT CẢ BÀI THI (Cả Lesson-level và Course-level)
        var allCourseQuizzes = await _db.Quizzes
            .Where(q => q.CourseId == courseId && !q.IsDeleted)
            .ToListAsync();

        var passedQuizIds = new List<int>();
        bool allQuizzesPassedResult = true;
        foreach (var q in allCourseQuizzes)
        {
            // Kiểm tra Pass qua QuizAttempt (Dành cho Final Quiz / Chapter Quiz)
            var passedViaAttempt = await _db.QuizAttempts.AnyAsync(qa => qa.UserId == userId && qa.QuizId == q.Id && qa.IsPassed);
            
            // HOẶC Kiểm tra qua LessonProgress (Dành cho Mini Quiz gắn trực tiếp vào Lesson - Vì Mini Quiz không tạo Attempt)
            var passedViaLesson = !passedViaAttempt && await _db.Lessons.AnyAsync(l => l.QuizId == q.Id && 
                                  _db.LessonProgresses.Any(lp => lp.UserId == userId && lp.LessonId == l.Id && lp.IsCompleted));

            if (passedViaAttempt || passedViaLesson) passedQuizIds.Add(q.Id);
            else allQuizzesPassedResult = false;
        }

        // 3. Tính % hiển thị (Bao gồm cả Lessons và Quizzes)
        int totalItems = totalLessons + allCourseQuizzes.Count;
        int completedItems = completedLessons + passedQuizIds.Count;
        double progressPercent = totalItems > 0 ? (double)completedItems / totalItems * 100 : 0;

        // 4. Xác định trạng thái Hoàn thành (Theo yêu cầu: Chỉ cần làm hết Quizz là đủ)
        bool isCompleted;
        if (allCourseQuizzes.Any())
        {
            // Nếu có quiz: Ưu tiên Pass hết quiz.
            isCompleted = allQuizzesPassedResult;
        }
        else
        {
            // Nếu không có quiz: Phải hoàn thành hết bài học
            isCompleted = totalLessons == 0 || completedLessons >= totalLessons;
        }

        if (isCompleted) progressPercent = 100;

        return new CourseProgressResponse(
            courseId, course.Title, Math.Round(progressPercent, 1),
            completedLessons, totalLessons, 
            passedQuizIds.Count, allCourseQuizzes.Count,
            isCompleted, allQuizzesPassedResult, passedQuizIds
        );
    }

    public async Task<ProgressResponse> UpdateVideoPositionAsync(int userId, int lessonId, int positionSeconds, int watchedPercent)
    {
        var lesson = await _db.Lessons.FindAsync(lessonId);
        var progress = await _db.LessonProgresses.FirstOrDefaultAsync(lp => lp.UserId == userId && lp.LessonId == lessonId);
        if (progress == null) { progress = new LessonProgress { UserId = userId, LessonId = lessonId }; _db.LessonProgresses.Add(progress); }
        progress.PositionSeconds = positionSeconds;
        progress.WatchedPercent = Math.Max(progress.WatchedPercent, watchedPercent);
        await _db.SaveChangesAsync();

        bool isQuizPassed = false;
        if (lesson?.QuizId != null)
        {
            isQuizPassed = await _db.QuizAttempts.AnyAsync(qa => qa.UserId == userId && qa.QuizId == lesson.QuizId && qa.IsPassed);
        }

        return new ProgressResponse(lessonId, progress.IsCompleted, positionSeconds, progress.WatchedPercent, isQuizPassed, lesson?.QuizId.HasValue ?? false);
    }

    public async Task<List<CourseProgressResponse>> GetUserProgressAsync(int userId)
    {
        // Lấy danh sách CourseId từ Enrollments (bao gồm cả Approved và Completed)
        // Join với Courses để đảm bảo khóa học còn tồn tại (không bị Soft Deleted)
        var enrollments = await _db.Enrollments
            .Where(e => e.UserId == userId && (e.Status == "Approved" || e.Status == "Completed"))
            .Join(_db.Courses, 
                  e => e.CourseId, 
                  c => c.Id, 
                  (e, c) => e.CourseId)
            .ToListAsync();

        var result = new List<CourseProgressResponse>();
        foreach (var cId in enrollments)
        {
            var progress = await GetCourseProgressAsync(userId, cId);
            if (progress != null)
            {
                result.Add(progress);
            }
        }
        return result;
    }

    public async Task<List<ProgressResponse>> GetLessonProgressesAsync(int userId, int courseId)
    {
        var lessonIds = await _db.Modules
            .Where(m => m.CourseId == courseId && !m.IsDeleted)
            .SelectMany(m => m.Lessons)
            .Where(l => !l.IsDeleted)
            .Select(l => l.Id)
            .ToListAsync();

        var progresses = await _db.LessonProgresses
            .Include(lp => lp.Lesson)
            .Where(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId))
            .ToListAsync();

        var result = new List<ProgressResponse>();
        foreach (var lp in progresses)
        {
            // Kiểm tra Pass qua QuizAttempt (Final/Chapter Quiz)
            bool isQuizPassed = lp.Lesson.QuizId.HasValue && 
                                await _db.QuizAttempts.AnyAsync(qa => qa.UserId == userId && qa.QuizId == lp.Lesson.QuizId.Value && qa.IsPassed);
            
            // HOẶC Nếu là Mini Quiz gắn trực tiếp vào bài học và bài học đã xong (IsCompleted) 
            // thì cũng coi như passed để frontend hiện tích xanh / nút hoàn thành.
            if (!isQuizPassed && lp.Lesson.QuizId.HasValue && lp.IsCompleted)
            {
                isQuizPassed = true;
            }

            result.Add(new ProgressResponse(lp.LessonId, lp.IsCompleted, lp.PositionSeconds, lp.WatchedPercent, isQuizPassed, lp.Lesson.QuizId.HasValue));
        }
        return result;
    }

    public async Task<bool> CanAccessLessonAsync(int userId, int lessonId)
    {
        var lesson = await _db.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null || lesson.IsFreePreview) return true;
        var user = await _db.Users.FindAsync(userId);
        if (user?.Role == "Admin") return true;
        var enrollment = await _db.Enrollments
            .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == lesson.Module.CourseId && (e.Status == "Approved" || e.Status == "Completed"));
        if (enrollment == null) return false;

        var course = await _db.Courses.FindAsync(lesson.Module.CourseId);
        if (course != null && course.Level == 2 && (enrollment.AssignedById == null || enrollment.GroupEnrollId != null))
        {
            var level1CourseIds = await _db.Courses
                .Where(c => c.Level == 1 && !c.IsDeleted && c.IsPublished)
                .Select(c => c.Id)
                .ToListAsync();

            if (level1CourseIds.Any())
            {
                var completedLevel1Count = await _db.Enrollments
                    .CountAsync(e => e.UserId == userId && level1CourseIds.Contains(e.CourseId) && e.Status == "Completed");

                if (completedLevel1Count < level1CourseIds.Count) return false;
            }
        }
        var allLessons = await _db.Lessons.Where(l => l.Module.CourseId == lesson.Module.CourseId).OrderBy(l => l.Module.SortOrder).ThenBy(l => l.SortOrder).ToListAsync();
        var idx = allLessons.FindIndex(l => l.Id == lessonId);
        if (idx == 0) return true;
        return await _db.LessonProgresses.AnyAsync(lp => lp.UserId == userId && lp.LessonId == allLessons[idx - 1].Id && lp.IsCompleted);
    }
}