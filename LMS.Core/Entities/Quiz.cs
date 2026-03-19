namespace LMS.Core.Entities;

public class Quiz : BaseEntity
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    // [5] Nullable = không giới hạn thời gian
    public int? TimeLimitMinutes { get; set; }
    public int PassScore { get; set; } = 8;
    public int QuestionCount { get; set; } = 10;
    public bool ShuffleQuestions { get; set; } = true;
    public bool ShuffleAnswers { get; set; } = true;
    // [4] Giới hạn số lần thi
    public int? MaxAttemptsPerWindow { get; set; }  // NULL = không giới hạn
    public int? AttemptWindowHours { get; set; }    // Cửa sổ thời gian (giờ)
    // [7] Thời hạn thi
    public int? AvailableFromDays { get; set; }     // Mở sau N ngày kể từ enroll
    public DateTime? QuizStartDate { get; set; }
    public DateTime? QuizEndDate { get; set; }

    // [MỚI] Ràng buộc làm lại cho riêng bài thi này (Nếu NULL thì lấy theo Course)
    public int? RetakeWaitTimeMinutes { get; set; }
    public int? MaxRetakesPerDay { get; set; }

    public ICollection<QuestionBank> Questions { get; set; } = new List<QuestionBank>(); 
    public Course Course { get; set; } = null!;
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
