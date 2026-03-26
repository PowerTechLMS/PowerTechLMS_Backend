using LMS.Core.Interfaces;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CourseEdit")]
public class AiController : ControllerBase
{
    private readonly ILlmService _llm;
    private readonly AppDbContext _db;

    public AiController(ILlmService llm, AppDbContext db)
    {
        _llm = llm;
        _db = db;
    }

    [HttpPost("suggest-content")]
    public async Task<IActionResult> SuggestContent([FromBody] SuggestContentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Tiêu đề không được để trống." });
        }

        var systemPrompt = 
            "Bạn là một chuyên gia soạn thảo giáo trình học thuật chuyên nghiệp. " +
            "Hãy viết nội dung chi tiết cho một bài học dựa trên tiêu đề và ngữ cảnh được cung cấp. " +
            "YÊU CẦU QUAN TRỌNG: CHỈ trả về nội dung bài học chính yếu. " +
            "TUYỆT ĐỐI KHÔNG kèm theo bất kỳ lời chào nào (ví dụ: 'Chào bạn...', 'Tôi xin trình bày...') ở đầu, " +
            "KHÔNG kèm lời dẫn dắt, KHÔNG kèm lời kết (ví dụ: 'Hy vọng...', 'Chúc bạn...') " +
            "và KHÔNG tự giới thiệu vai trò của mình. " +
            "Nội dung cần trình bày dưới dạng Markdown, sử dụng các thẻ tiêu đề (##, ###), danh sách và định dạng in đậm để dễ đọc. " +
            "Hãy viết bằng tiếng Việt, phong cách giảng dạy chuyên sâu và thực tế.";

        var userPrompt = $"Tiêu đề bài học: {request.Title}";
        if (!string.IsNullOrWhiteSpace(request.Context))
        {
            userPrompt += $"\nYêu cầu thêm/Ngữ cảnh: {request.Context}";
        }

        try
        {
            var suggestedContent = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);
            return Ok(new { content = suggestedContent });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gọi AI: " + ex.Message });
        }
    }

    [HttpPost("generate-quiz")]
    public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.Content))
            return BadRequest(new { message = "Nội dung bài học không được để trống." });

        int count = request.Count <= 0 ? 5 : request.Count;

        var systemPrompt = 
            "Bạn là một chuyên gia khảo thí và xây dựng bộ câu hỏi học thuật. " +
            "Dựa trên nội dung bài học được cung cấp, hãy tạo ra các câu hỏi trắc nghiệm khách quan (Multiple Choice Questions) với 4 lựa chọn A, B, C, D. " +
            "Yêu cầu:\n" +
            "1. Các câu hỏi phải bám sát nội dung bài học.\n" +
            "2. Chỉ có duy nhất một đáp án đúng.\n" +
            "3. Phải cung cấp lời giải thích ngắn gọn bằng tiếng Việt cho đáp án đúng.\n" +
            "4. Định dạng đầu ra PHẢI là một mảng JSON hợp lệ chứa các đối tượng có cấu trúc:\n" +
            "   { \"questionText\": \"...\", \"optionA\": \"...\", \"optionB\": \"...\", \"optionC\": \"...\", \"optionD\": \"...\", \"correctAnswer\": \"A/B/C/D\", \"explanation\": \"...\" }\n" +
            "5. Không trả về bất kỳ văn bản nào khác ngoài JSON mảng. Không bao gồm ```json ... ```.";

        var userPrompt = $"Số lượng câu hỏi cần tạo: {count}\n" +
                         $"Nội dung bài học:\n{request.Content}\n" +
                         (string.IsNullOrWhiteSpace(request.Context) ? "" : $"Lưu ý thêm/Phong cách: {request.Context}");

        try
        {
            var response = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);
            var json = CleanJsonResponse(response);
            return Ok(new { questionsJson = json });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gọi AI: " + ex.Message });
        }
    }

    [HttpPost("generate-lesson-quiz")]
    public async Task<IActionResult> GenerateLessonQuiz([FromBody] GenerateLessonQuizRequest request)
    {
        if (request.LessonId <= 0)
            return BadRequest(new { message = "LessonId không hợp lệ." });

        var lesson = await _db.Lessons.FindAsync(request.LessonId);
        if (lesson == null)
            return NotFound(new { message = "Không tìm thấy bài học." });

        var content = lesson.Transcript ?? lesson.AiSummary ?? lesson.Content;
        if (string.IsNullOrWhiteSpace(content) || content.Length < 50)
            return BadRequest(new { message = "Nội dung bài học quá ngắn hoặc trống để AI có thể tạo câu hỏi chất lượng." });

        int count = request.Count <= 0 ? 5 : request.Count;

        var systemPrompt = 
            "Bạn là một chuyên gia khảo thí và xây dựng bộ câu hỏi học thuật. " +
            "Dựa trên nội dung bài học được cung cấp, hãy tạo ra các câu hỏi trắc nghiệm khách quan (Multiple Choice Questions) với 4 lựa chọn A, B, C, D. " +
            "Yêu cầu:\n" +
            "1. Các câu hỏi phải bám sát nội dung bài học.\n" +
            "2. Chỉ có duy nhất một đáp án đúng.\n" +
            "3. Phải cung cấp lời giải thích ngắn gọn bằng tiếng Việt cho đáp án đúng.\n" +
            "4. Định dạng đầu ra PHẢI là một mảng JSON hợp lệ chứa các đối tượng có cấu trúc:\n" +
            "   { \"questionText\": \"...\", \"optionA\": \"...\", \"optionB\": \"...\", \"optionC\": \"...\", \"optionD\": \"...\", \"correctAnswer\": \"A/B/C/D\", \"explanation\": \"...\" }\n" +
            "5. Không trả về bất kỳ văn bản nào khác ngoài JSON mảng. Không bao gồm ```json ... ```.";

        var userPrompt = $"Số lượng câu hỏi cần tạo: {count}\n" +
                         $"Nội dung bài học:\n{content}\n" +
                         (string.IsNullOrWhiteSpace(request.Context) ? "" : $"Lưu ý thêm/Phong cách: {request.Context}");

        try
        {
            var response = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);
            var json = CleanJsonResponse(response);
            return Ok(new { questionsJson = json });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gọi AI: " + ex.Message });
        }
    }

    [HttpPost("generate-course-quiz")]
    public async Task<IActionResult> GenerateCourseQuiz([FromBody] GenerateCourseQuizRequest request)
    {
        if (request?.Lessons == null || request.Lessons.Count == 0)
            return BadRequest(new { message = "Danh sách bài học không được để trống." });

        var systemPrompt = 
            "Bạn là một chuyên gia khảo thí và xây dựng bộ đề thi tổng hợp. " +
            "Dựa trên danh sách nội dung các bài học trong khóa học, hãy tạo ra bộ câu hỏi trắc nghiệm khách quan tổng hợp (ABCD). " +
            "Yêu cầu:\n" +
            "1. Cố gắng phân bổ câu hỏi đều cho các bài học dựa trên số lượng được yêu cầu.\n" +
            "2. Chỉ có duy nhất một đáp án đúng.\n" +
            "3. Phải cung cấp lời giải thích ngắn gọn bằng tiếng Việt cho đáp án đúng.\n" +
            "4. Định dạng đầu ra PHẢI là một mảng JSON hợp lệ chứa các đối tượng có cấu trúc:\n" +
            "   { \"questionText\": \"...\", \"optionA\": \"...\", \"optionB\": \"...\", \"optionC\": \"...\", \"optionD\": \"...\", \"correctAnswer\": \"A/B/C/D\", \"explanation\": \"...\" }\n" +
            "5. Không trả về bất kỳ văn bản nào khác ngoài JSON mảng. Không bao gồm ```json ... ```.";

        var contentBuilder = new System.Text.StringBuilder();
        foreach (var item in request.Lessons)
        {
            var lesson = await _db.Lessons.FindAsync(item.LessonId);
            if (lesson == null) continue;

            var content = lesson.Transcript ?? lesson.AiSummary ?? lesson.Content;
            if (string.IsNullOrWhiteSpace(content)) continue;

            contentBuilder.AppendLine($"--- BÀI HỌC: {lesson.Title} (Số câu đề nghị: {item.Count}) ---");
            contentBuilder.AppendLine(content);
            contentBuilder.AppendLine();
        }

        if (contentBuilder.Length == 0)
            return BadRequest(new { message = "Không tìm thấy nội dung bài giảng hợp lệ để tạo đề thi." });

        var userPrompt = $"Tổng hợp nội dung khóa học:\n{contentBuilder}\n" +
                         (string.IsNullOrWhiteSpace(request.GlobalContext) ? "" : $"Lưu ý chung cho đề thi: {request.GlobalContext}");

        try
        {
            var response = await _llm.GenerateResponseAsync(systemPrompt, userPrompt);
            var json = CleanJsonResponse(response);
            return Ok(new { questionsJson = json });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gọi AI: " + ex.Message });
        }
    }

    private string CleanJsonResponse(string response)
    {
        var json = response.Trim();
        if (json.StartsWith("```json")) json = json.Substring(7);
        else if (json.StartsWith("```")) json = json.Substring(3);
        
        if (json.EndsWith("```")) json = json.Substring(0, json.Length - 3);
        
        return json.Trim();
    }
}

public class GenerateLessonQuizRequest
{
    public int LessonId { get; set; }
    public int Count { get; set; }
    public string? Context { get; set; }
}

public class SuggestContentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Context { get; set; }
}

public class GenerateQuizRequest
{
    public string Content { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? Context { get; set; }
}

public class GenerateCourseQuizRequest
{
    public List<LessonQuizRequestItem> Lessons { get; set; } = new();
    public string? GlobalContext { get; set; }
}

public class LessonQuizRequestItem
{
    public int LessonId { get; set; }
    public int Count { get; set; }
}
