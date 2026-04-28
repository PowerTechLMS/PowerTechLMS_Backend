using LMS.Core.Entities;
using LMS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedDataController : ControllerBase
{
    private readonly AppDbContext _db;

    public SeedDataController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("seed-essays")]
    public async Task<IActionResult> SeedEssays([FromBody] List<int> excludedUserIds)
    {
        excludedUserIds ??= new List<int>();

        // 1. Tìm tất cả các EssayConfig chỉ có đúng 1 câu hỏi chưa bị xóa
        var validConfigs = await _db.EssayConfigs
            .Include(c => c.Questions)
            .Include(c => c.Lesson)
            .Where(c => !c.IsDeleted && c.Lesson.Type == "Essay")
            .ToListAsync();

        var targets = validConfigs
            .Where(c => c.Questions.Count(q => !q.IsDeleted) == 1)
            .Select(c => new 
            { 
                ConfigId = c.Id, 
                LessonId = c.LessonId, 
                QuestionId = c.Questions.First(q => !q.IsDeleted).Id 
            })
            .ToList();

        if (!targets.Any())
        {
            return BadRequest("Không tìm thấy bài giảng tự luận nào có đúng 1 câu hỏi.");
        }

        // 2. Lấy danh sách người dùng không nằm trong danh sách loại trừ
        var users = await _db.Users
            .Where(u => !u.IsDeleted && !excludedUserIds.Contains(u.Id))
            .ToListAsync();

        if (!users.Any())
        {
            return BadRequest("Không tìm thấy người dùng hợp lệ để gán bài làm.");
        }

        // 3. Chuẩn bị 3 bài mẫu
        string[] samples = new string[]
        {
            @"Xung đột tại môi trường công sở là điều không thể tránh khỏi bởi vì mỗi cá nhân đều mang theo những hệ giá trị, kinh nghiệm và phong cách làm việc riêng biệt. Khi những sự khác biệt này va chạm trong một không gian chung để thực hiện mục tiêu tập thể, sự bất đồng là kết quả tất yếu của quá trình tương tác xã hội.

Bên cạnh đó, sự giới hạn về nguồn lực như ngân sách, nhân sự hay quyền lợi cũng là mồi lửa cho các tranh chấp. Trong một hệ thống mà lợi ích của người này có thể ảnh hưởng đến người kia, việc cạnh tranh để giành lấy sự ưu tiên thường dẫn đến những căng thẳng về mặt lợi ích và quyền lực.

Về mặt hậu quả, nếu mâu thuẫn không được giải quyết khéo léo, nó sẽ tạo ra một bầu không khí làm việc độc hại và đầy rẫy sự nghi kỵ. Niềm tin giữa các đồng nghiệp bị xói mòn khiến dòng chảy thông tin bị tắc nghẽn, làm tê liệt khả năng phối hợp và sáng tạo của cả nhóm.

Cuối cùng, xung đột kéo dài sẽ dẫn đến tình trạng chảy máu chất xám khi những nhân viên giỏi cảm thấy kiệt sức và chọn cách rời đi. Điều này không chỉ làm tiêu hao tài chính của tổ chức cho việc tuyển dụng lại mà còn phá hủy uy tín và sức mạnh cạnh tranh của tập thể trên thị trường.",

            @"Bên cạnh đó, sự giới hạn về nguồn lực như ngân sách, nhân sự hay quyền lợi cũng là mồi lửa cho các tranh chấp. Trong một hệ thống mà lợi ích của người này có thể ảnh hưởng đến người kia, việc cạnh tranh để giành lấy sự ưu tiên thường dẫn đến những căng thẳng về mặt lợi ích và quyền lực.

Xung đột tại không gian văn phòng là điều không thể tránh khỏi bởi vì mỗi cá nhân đều mang theo những hệ giá trị, kinh nghiệm và phong cách làm việc riêng biệt. Khi những sự khác biệt này va chạm trong một không gian chung để thực hiện mục tiêu tập thể, sự bất đồng là kết quả tất yếu của quá trình tương tác xã hội.

Cuối cùng, xung đột kéo dài sẽ dẫn đến tình trạng chảy máu chất xám khi những nhân viên giỏi cảm thấy kiệt sức và chọn cách rời đi. Điều này không chỉ làm tiêu hao tài chính của tổ chức cho việc tuyển dụng lại mà còn phá hủy uy tín và sức mạnh cạnh tranh của tập thể trên thị trường.

Về mặt hậu quả, nếu mâu thuẫn không được giải quyết khéo léo, nó sẽ tạo ra một bầu không khí làm việc độc hại và đầy rẫy sự nghi kỵ. Niềm tin giữa các đồng nghiệp bị xói mòn khiến dòng chảy thông tin bị tắc nghẽn, làm tê liệt khả năng phối hợp và sáng tạo của cả nhóm.",

            @"Xung đột kéo dài sẽ dẫn đến tình trạng chảy máu chất xám khi những nhân viên giỏi cảm thấy kiệt sức và chọn cách rời đi. Điều này không chỉ làm tiêu hao tài chính của tổ chức cho việc tuyển dụng lại mà còn phá hủy uy tín và sức mạnh cạnh tranh của tập thể trên thị trường.

Về mặt hậu quả, nếu mâu thuẫn không được giải quyết khéo léo, nó sẽ tạo ra một bầu không khí làm việc độc hại và đầy rẫy sự nghi kỵ. Niềm tin giữa các đồng nghiệp bị xói mòn khiến dòng chảy thông tin bị tắc nghẽn, làm tê liệt khả năng phối hợp và sáng tạo của cả nhóm.

Bên cạnh đó, sự giới hạn về nguồn lực như ngân sách, nhân sự hay quyền lợi cũng là mồi lửa cho các tranh chấp. Trong một hệ thống mà lợi ích của người này có thể ảnh hưởng đến người kia, việc cạnh tranh để giành lấy sự ưu tiên thường dẫn đến những căng thẳng về mặt lợi ích và quyền lực.

Xung đột tại **địa bàn công tác** là điều không thể tránh khỏi bởi vì mỗi cá nhân đều mang theo những hệ giá trị, kinh nghiệm và phong cách làm việc riêng biệt. Khi những sự khác biệt này va chạm trong một không gian chung để thực hiện mục tiêu tập thể, sự bất đồng là kết quả tất yếu của quá trình tương tác xã hội.

Theo các học thuyết về quản trị nhân sự hiện đại, xung đột được định nghĩa là một quá trình trong đó một bên nhận thấy rằng quyền lợi của mình đang bị bên kia phản đối hoặc ảnh hưởng tiêu cực. Xung đột không thuần túy là sự đối đầu trực diện mà còn bao gồm các trạng thái tâm lý như lo âu, thất vọng và sự thù địch ngầm giữa các thành viên trong cùng một hệ thống vận hành."
        };

        var random = new Random();
        var attemptsCreated = 0;

        // 4. Tạo 100 bài làm
        for (int i = 0; i < 100; i++)
        {
            var user = users[random.Next(users.Count)];
            var target = targets[random.Next(targets.Count)];
            var content = samples[random.Next(samples.Length)];

            var attempt = new EssayAttempt
            {
                UserId = user.Id,
                LessonId = target.LessonId,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddMinutes(-30),
                TotalScore = random.Next(40, 95)
            };

            attempt.Answers.Add(new EssayAnswer
            {
                QuestionId = target.QuestionId,
                Content = content,
                AiScore = attempt.TotalScore
            });

            _db.EssayAttempts.Add(attempt);
            attemptsCreated++;
        }

        await _db.SaveChangesAsync();

        return Ok(new { Message = $"Đã tạo thành công 100 lượt làm bài.", TargetsCount = targets.Count });
    }
}
