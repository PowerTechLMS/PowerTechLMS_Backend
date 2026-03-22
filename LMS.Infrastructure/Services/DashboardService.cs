using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace LMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<LearnerDashboardResponse> GetLearnerDashboardAsync(int userId)
    {
        var response = new LearnerDashboardResponse();
        var now = DateTime.UtcNow;

        try
        {
            var userGroup = await _db.UserGroupMembers
                .Include(ugm => ugm.Group)
                .FirstOrDefaultAsync(ugm => ugm.UserId == userId && !ugm.IsDeleted);
            response.DepartmentName = userGroup?.Group?.Name ?? "Chưa phân bổ";

            var enrollments = await _db.Enrollments
                .AsSplitQuery()
                .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .Where(e => e.UserId == userId && !e.IsDeleted && (e.Status == "Approved" || e.Status == "Completed"))
                .OrderByDescending(e => e.UpdatedAt)
                .ToListAsync();

            var progresses = await _db.LessonProgresses
                .Include(lp => lp.Lesson)
                .Where(lp => lp.UserId == userId && !lp.IsDeleted)
                .ToListAsync();

            var quizAttempts = await _db.QuizAttempts.Where(qa => qa.UserId == userId && !qa.IsDeleted).ToListAsync();

            int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeek = now.AddDays(-1 * diff).Date;
            response.WeekMission.WeekLabel = $"Tuần {CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)}";
            response.WeekMission.MandatoryTotal = enrollments.Count(e => e.IsMandatory);
            response.WeekMission.MandatoryDone = enrollments.Count(e => e.IsMandatory && e.Status == "Completed");
            response.WeekMission.OptionalTotal = enrollments.Count(e => !e.IsMandatory);
            response.WeekMission.OptionalDone = enrollments.Count(e => !e.IsMandatory && e.Status == "Completed");
            response.WeekMission.TotalTasks = response.WeekMission.MandatoryTotal + response.WeekMission.OptionalTotal;
            response.WeekMission.DoneTasks = response.WeekMission.MandatoryDone + response.WeekMission.OptionalDone;

            var lessonsDone = progresses.Count(p => p.IsCompleted && p.CompletedAt >= startOfWeek);
            var quizzesDone = quizAttempts.Count(q => q.IsPassed && q.SubmittedAt >= startOfWeek);
            response.WeekMission.XpEarned = (lessonsDone * 10) + (quizzesDone * 50);
            response.WeekMission.XpTarget = 500;

            if(response.WeekMission.XpEarned >= response.WeekMission.XpTarget)
                response.WeekMission.Status = "Bạn đang đi đúng tiến độ 👍";
            else if(response.WeekMission.XpEarned > 0)
                response.WeekMission.Status = $"Còn {response.WeekMission.XpTarget - response.WeekMission.XpEarned} XP để đạt mục tiêu.";
            else
                response.WeekMission.Status = "Bắt đầu bài học đầu tiên ngay!";

            var activeDates = progresses.Where(p => p.CompletedAt.HasValue)
                .Select(p => p.CompletedAt!.Value.Date)
                .Union(quizAttempts.Where(q => q.SubmittedAt.HasValue).Select(q => q.SubmittedAt!.Value.Date))
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            int streak = 0;
            var checkDate = now.Date;
            if(activeDates.Contains(checkDate) || activeDates.Contains(checkDate.AddDays(-1)))
            {
                if(!activeDates.Contains(checkDate))
                    checkDate = checkDate.AddDays(-1);
                foreach(var d in activeDates)
                {
                    if(d == checkDate)
                    {
                        streak++;
                        checkDate = checkDate.AddDays(-1);
                    } else if(d < checkDate)
                        break;
                }
            }
            response.WeekMission.Streak = streak;

            int totalPct = response.WeekMission.TotalTasks > 0
                ? (response.WeekMission.DoneTasks * 100) / response.WeekMission.TotalTasks
                : 0;
            response.WeekMission.Skills = new List<SkillProgressDto>
            {
                new("Kỹ năng chuyên môn", Math.Min(totalPct + 15, 100), "#6366f1"),
                new("Kỹ năng mềm", Math.Min(totalPct + 5, 100), "#10b981"),
                new("Tuân thủ", totalPct, "#f59e0b")
            };

            foreach(var e in enrollments)
            {
                if(e.Course == null)
                    continue;
                var allLessons = e.Course.Modules?.SelectMany(m => m.Lessons ?? new List<Lesson>())
                        .OrderBy(l => l.SortOrder)
                        .ToList() ??
                    new();
                var doneIds = progresses.Where(p => p.IsCompleted).Select(p => p.LessonId).ToList();
                var compCount = allLessons.Count(l => doneIds.Contains(l.Id));
                double progPct = allLessons.Count == 0 ? 0 : Math.Round(((double)compCount / allLessons.Count) * 100);

                var nextL = allLessons.FirstOrDefault(l => !doneIds.Contains(l.Id));
                string? nextTitle = nextL?.Title;
                string? estTime = nextL != null
                    ? $"{Math.Max(5, (nextL.VideoDurationSeconds + nextL.ReadingDurationSeconds) / 60)} phút"
                    : "Hoàn tất";

                if(response.MyCourses.Count < 6)
                {
                    response.MyCourses
                        .Add(
                            new DashboardCourseDto(
                                e.CourseId,
                                e.Course.Title,
                                e.IsMandatory ? "Bắt buộc" : "Gợi ý",
                                $"{compCount}/{allLessons.Count}",
                                "0/1",
                                "#6366f1",
                                "#a855f7",
                                progPct,
                                compCount,
                                allLessons.Count,
                                nextTitle,
                                estTime));
                }

                if(compCount < allLessons.Count && response.TodayTasks.Count < 5)
                {
                    response.TodayTasks
                        .Add(
                            new DashboardTaskDto(
                                e.Id,
                                $"Bài: {nextTitle ?? e.Course.Title}",
                                $"{compCount}/{allLessons.Count}",
                                $"{progPct}%",
                                false,
                                false));
                }
            }

            var cIds = enrollments.Select(e => e.CourseId).ToList();
            var qzs = await _db.Quizzes
                .Where(q => cIds.Contains(q.CourseId) && !q.IsDeleted)
                .OrderByDescending(q => q.CreatedAt)
                .Take(5)
                .ToListAsync();
            foreach(var q in qzs)
            {
                var att = quizAttempts.FirstOrDefault(a => a.QuizId == q.Id);
                string stat = att == null ? "new" : (att.IsPassed ? "done" : "doing");
                if(response.TestPractice.Count < 4)
                    response.TestPractice
                        .Add(
                            new DashboardQuizDto(
                                q.Id,
                                q.Title,
                                $"{q.QuestionCount} câu hỏi",
                                stat,
                                stat == "new" ? "Chưa làm" : (stat == "done" ? "Hoàn thành" : "Đang làm"),
                                "#6366f1"));
            }

            double entryScore = 3.0;
            double targetScore = 5.0;
            long totalSecs = progresses.Where(p => p.IsCompleted && p.Lesson != null)
                .Sum(p => (long)p.Lesson!.VideoDurationSeconds + p.Lesson!.ReadingDurationSeconds);

            response.LearningProfile.Dept = response.DepartmentName;
            response.LearningProfile.EntryScore = entryScore;
            response.LearningProfile.TargetScore = targetScore;
            response.LearningProfile.PredictedScore = Math.Round(
                entryScore + ((double)totalPct / 100 * (targetScore - entryScore)),
                1);
            response.LearningProfile.Summary = new List<SummaryStatDto>
            {
                new("Giờ học", $"{Math.Round((double)totalSecs / 3600, 1)}h", "#6366f1"),
                new("XP Tuần", $"{response.WeekMission.XpEarned}", "#f59e0b"),
                new("Chứng chỉ", $"{enrollments.Count(e => e.Status == "Completed")}", "#10b981")
            };

            var alerts = enrollments.Where(
                e => e.IsMandatory && e.Deadline.HasValue && e.Deadline.Value < now && e.Status != "Completed")
                .Take(3)
                .ToList();
            foreach(var a in alerts)
                response.HrMessages
                    .Add(new HrMessageDto(a.Id, "HR", $"Quá hạn: {a.Course?.Title ?? "Khóa học"}", "Gấp", true));
        } catch(Exception ex)
        {
            Console.WriteLine($"Dashboard Error: {ex.Message}");
        }
        return response;
    }
}
