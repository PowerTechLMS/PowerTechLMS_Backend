using LMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

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
    [HttpGet("learner")]
    public IActionResult GetLearnerDashboard()
    {
        // Khởi tạo dữ liệu trả về khớp hoàn toàn với cấu trúc VueJS yêu cầu
        var dashboardData = new
        {
            // 1. Nhiệm vụ hôm nay
            TodayTasks = new[]
            {
                new { Id = 1, Title = "Hoàn thành Bài 3: Kỹ năng giao tiếp", Sub = "Khóa học Kỹ năng mềm", Done = true, Locked = false, Progress = "" },
                new { Id = 2, Title = "Làm bài kiểm tra năng lực", Sub = "Hạn chót: Hôm nay", Done = false, Locked = false, Progress = "0/15 Câu" },
                new { Id = 3, Title = "Xem Video: An toàn lao động", Sub = "Chưa mở khóa", Done = false, Locked = true, Progress = "" }
            },

            // 2. Nhiệm vụ tuần
            WeekMission = new
            {
                WeekLabel = "Tuần 12 - 18/03",
                TotalTasks = 5,
                DoneTasks = 3,
                XpEarned = 350,
                XpTarget = 500,
                MandatoryDone = 2,
                MandatoryTotal = 3,
                OptionalDone = 1,
                OptionalTotal = 2,
                Status = "Đang bám sát tiến độ!",
                StatusType = "ok", // 'ok' hoặc 'warn'
                Skills = new[]
                {
                    new { Name = "Giao tiếp", Pct = 80, Color = "#10b981" },
                    new { Name = "Chuyên môn", Pct = 45, Color = "#f59e0b" }
                }
            },

            // 3. Khóa học của tôi
            MyCourses = new[]
            {
                new { Id = 1, Title = "Kỹ năng Lãnh đạo Cơ bản", ProgressPercent = 65, Units = 12, Cups = 2, ThumbColor = "#4f46e5", ThumbColor2 = "#818cf8", Tag = "BẮT BUỘC" },
                new { Id = 2, Title = "Bảo mật thông tin doanh nghiệp", ProgressPercent = 10, Units = 5, Cups = 0, ThumbColor = "#10b981", ThumbColor2 = "#34d399", Tag = "TỰ CHỌN" }
            },

            // 4. Bài kiểm tra cần làm
            TestPractice = new[]
            {
                new { Id = 101, Title = "Đánh giá năng lực Quý 1", Sub = "15 phút • 20 câu hỏi", Cover = "linear-gradient(135deg, #f59e0b, #ef4444)", Status = "doing", Badge = "ĐANG LÀM" }
            },

            // 5. Hồ sơ năng lực
            LearningProfile = new
            {
                Dept = "Phòng IT",
                Entry = "Khởi điểm",
                EntryScore = 55,
                PredictedScore = 72,
                TargetScore = 85,
                Summary = new[]
                {
                    new { Label = "Tổng giờ học", Val = "24h 15m", Color = "#111827" },
                    new { Label = "Khóa hoàn thành", Val = "3", Color = "#10b981" },
                    new { Label = "Điểm trung bình", Val = "8.5", Color = "#4f46e5" }
                }
            },

            // 6. Nhắc nhở từ HR
            HrMessages = new[]
            {
                new { Id = 1, From = "Phòng Nhân sự", Msg = "Vui lòng hoàn thành khóa An toàn lao động trước 20/03.", Time = "2 giờ trước", Unread = true }
            }
        };

        return Ok(dashboardData);
    }
}