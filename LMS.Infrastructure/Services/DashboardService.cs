using LMS.Core.DTOs;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<LearnerDashboardResponse> GetLearnerDashboardAsync(int userId)
    {
        var response = new LearnerDashboardResponse();
        var now = DateTime.UtcNow;

        // 1. Lấy thông tin Phòng ban
        var userGroup = await _db.UserGroupMembers
            .Include(ugm => ugm.Group)
            .FirstOrDefaultAsync(ugm => ugm.UserId == userId && !ugm.IsDeleted);
        response.DepartmentName = userGroup?.Group?.Name ?? "Chưa phân bổ";

        // 2. Lấy dữ liệu Ghi danh (Kèm an toàn Null)
        var enrollments = await _db.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
            .Where(e => e.UserId == userId && !e.IsDeleted)
            .ToListAsync();

        // 3. Lấy dữ liệu Tiến độ học
        var progresses = await _db.LessonProgresses
            .Include(lp => lp.Lesson)
            .Where(lp => lp.UserId == userId && !lp.IsDeleted)
            .ToListAsync();

        // 4. Lấy dữ liệu bài kiểm tra
        var quizAttempts = await _db.QuizAttempts
            .Where(qa => qa.UserId == userId && !qa.IsDeleted)
            .ToListAsync();

        // --- TÍNH TOÁN WEEKLY MISSION (AN TOÀN DATE) ---
        int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
        var startOfWeek = now.AddDays(-1 * diff).Date;
        var endOfWeek = startOfWeek.AddDays(7);

        response.WeekMission.WeekLabel = $"Tuần {GetIso8601WeekOfYear(now)} — Tháng {now.Month}/{now.Year}";
        response.WeekMission.MandatoryTotal = enrollments.Count(e => e.IsMandatory);
        response.WeekMission.MandatoryDone = enrollments.Count(e => e.IsMandatory && e.Status == "Completed");
        response.WeekMission.OptionalTotal = enrollments.Count(e => !e.IsMandatory);
        response.WeekMission.OptionalDone = enrollments.Count(e => !e.IsMandatory && e.Status == "Completed");

        response.WeekMission.TotalTasks = response.WeekMission.MandatoryTotal + response.WeekMission.OptionalTotal;
        response.WeekMission.DoneTasks = response.WeekMission.MandatoryDone + response.WeekMission.OptionalDone;

        // Tính XP
        var lessonsDoneThisWeek = progresses.Count(p => p.IsCompleted && p.CompletedAt >= startOfWeek && p.CompletedAt < endOfWeek);
        var quizzesPassedThisWeek = quizAttempts.Count(q => q.IsPassed && q.SubmittedAt >= startOfWeek && q.SubmittedAt < endOfWeek);
        response.WeekMission.XpEarned = (lessonsDoneThisWeek * 10) + (quizzesPassedThisWeek * 50);
        response.WeekMission.XpTarget = 500;
        response.WeekMission.Streak = progresses.Where(p => p.CompletedAt.HasValue).Select(p => p.CompletedAt!.Value.Date).Distinct().Count();

        // Kỹ năng
        int totalProgressPct = response.WeekMission.TotalTasks > 0 ? (response.WeekMission.DoneTasks * 100) / response.WeekMission.TotalTasks : 0;
        response.WeekMission.Skills = new List<SkillProgressDto>
        {
            new("Kỹ năng chuyên môn", Math.Min(totalProgressPct + 15, 100), "#818cf8"),
            new("Kỹ năng mềm", Math.Min(totalProgressPct + 5, 100), "#10b981"),
            new("Tuân thủ", totalProgressPct, "#f59e0b")
        };

        response.WeekMission.Status = totalProgressPct >= 50 ? "Đang tiến hành — bạn đang đi đúng hướng! 💪" : "Cố lên! Bạn cần nỗ lực nhiều hơn tuần này 🏃";
        response.WeekMission.StatusType = totalProgressPct >= 50 ? "ok" : "warn";

        // --- TÍNH TOÁN KHOÁ HỌC (AN TOÀN NULL CHO COURSE/MODULE) ---
        foreach (var e in enrollments.Where(e => e.Status != "Completed").Take(3))
        {
            if (e.Course == null) continue; // Tránh Null Course

            // Trích xuất Lesson an toàn
            var lessonIds = new List<int>();
            if (e.Course.Modules != null)
            {
                lessonIds = e.Course.Modules
                    .Where(m => m.Lessons != null)
                    .SelectMany(m => m.Lessons!)
                    .Select(l => l.Id)
                    .ToList();
            }

            var totalLessons = lessonIds.Count;
            var compLessons = progresses.Count(p => p.IsCompleted && lessonIds.Contains(p.LessonId));
            double progPct = totalLessons == 0 ? 0 : Math.Round(((double)compLessons / totalLessons) * 100);

            response.MyCourses.Add(new DashboardCourseDto(
                e.CourseId, e.Course.Title, "Đang học", $"{compLessons}/{totalLessons}", "0/1", "#818cf8", "#a78bfa", progPct, compLessons, totalLessons
            ));
        }

        // --- TÍNH TOÁN BÀI KIỂM TRA ---
        var courseIds = enrollments.Select(e => e.CourseId).ToList();
        var pendingQuizzes = await _db.Quizzes
            .Where(q => courseIds.Contains(q.CourseId) && !q.IsDeleted)
            .Take(2)
            .ToListAsync();

        foreach (var q in pendingQuizzes)
        {
            var attempt = quizAttempts.FirstOrDefault(a => a.QuizId == q.Id);
            string status = attempt == null ? "new" : (attempt.IsPassed ? "done" : "doing");
            string badge = attempt == null ? "Mới" : (attempt.IsPassed ? "Hoàn thành" : "Đang làm");
            response.TestPractice.Add(new DashboardQuizDto(q.Id, q.Title, $"{q.QuestionCount} câu hỏi", status, badge, "#818cf8"));
        }

        // --- TÍNH TOÁN THỐNG KÊ (AN TOÀN CHO VIDEO DURATION) ---
        var totalSeconds = progresses.Where(p => p.IsCompleted && p.Lesson != null).Sum(p => p.Lesson!.VideoDurationSeconds);

        response.LearningProfile.Dept = response.DepartmentName;
        response.LearningProfile.PredictedScore = Math.Round(3.0 + ((double)totalProgressPct / 50), 1);

        response.LearningProfile.Summary = new List<SummaryStatDto>
        {
            new("Tổng giờ học", $"{Math.Round((double)totalSeconds / 3600, 1)} giờ", "#818cf8"),
            new("Tổng XP", $"{response.WeekMission.XpEarned}", "#f59e0b"),
            new("Tổng bài thi", $"{quizAttempts.Count}", "#ef4444"),
            new("Bài giảng hoàn thành", $"{progresses.Count(p => p.IsCompleted)}", "#10b981")
        };

        // --- HR MESSAGES (AN TOÀN CHO COURSE TITLE) ---
        var overdue = enrollments.Where(e => e.IsMandatory && e.Deadline.HasValue && e.Deadline.Value < now && e.Status != "Completed").ToList();
        foreach (var o in overdue)
        {
            string courseName = o.Course?.Title ?? "Không xác định";
            response.HrMessages.Add(new HrMessageDto(o.Id, "Hệ thống", $"Khóa học bắt buộc '{courseName}' đã quá hạn vào {o.Deadline:dd/MM/yyyy}.", "Ngay bây giờ", true));
        }

        return response;
    }

    private int GetIso8601WeekOfYear(DateTime time)
    {
        DayOfWeek day = System.Globalization.CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
        if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday) time = time.AddDays(3);
        return System.Globalization.CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
    }
}