using System.ComponentModel.DataAnnotations.Schema; // BẮT BUỘC THÊM DÒNG NÀY

namespace LMS.Core.Entities;

public class QuestionBank : BaseEntity
{
    public int Id { get; set; }
    public int QuizId { get; set; } // Khóa ngoại chuẩn 
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = "A";
    public decimal Points { get; set; } = 1.0m; // Trọng số câu hỏi

    [ForeignKey("QuizId")]
    public Quiz Quiz { get; set; }
    public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
}