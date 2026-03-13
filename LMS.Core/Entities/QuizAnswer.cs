namespace LMS.Core.Entities;

public class QuizAnswer : BaseEntity
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    // [6] Nullable = chưa trả lời (lưu nháp bài đang làm)
    public string? SelectedAnswer { get; set; }
    // NULL = chưa chấm (chưa submit)
    public bool? IsCorrect { get; set; }
    public DateTime? AnsweredAt { get; set; }

    public QuizAttempt Attempt { get; set; } = null!;
    public QuestionBank Question { get; set; } = null!;
}
