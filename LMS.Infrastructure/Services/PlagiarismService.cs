using LMS.Core.DTOs;
using LMS.Core.Entities;
using LMS.Core.Interfaces;
using LMS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace LMS.Infrastructure.Services;

public class PlagiarismService : IPlagiarismService
{
    private readonly AppDbContext _db;
    private readonly ILlmService _llmService;
    private const int NgramSize = 5;

    public PlagiarismService(AppDbContext db, ILlmService llmService)
    {
        _db = db;
        _llmService = llmService;
    }

    public async Task<PlagiarismCompareResponse> CompareAsync(PlagiarismCompareRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var attemptA = await _db.EssayAttempts
            .Include(a => a.User)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptIdA) ??
            throw new KeyNotFoundException($"Không tìm thấy bài nộp với Id={request.AttemptIdA}");

        var attemptB = await _db.EssayAttempts
            .Include(a => a.User)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptIdB) ??
            throw new KeyNotFoundException($"Không tìm thấy bài nộp với Id={request.AttemptIdB}");

        var answerA = attemptA.Answers.FirstOrDefault(ans => ans.QuestionId == request.QuestionId);
        var answerB = attemptB.Answers.FirstOrDefault(ans => ans.QuestionId == request.QuestionId);

        var textA = answerA?.Content ?? string.Empty;
        var textB = answerB?.Content ?? string.Empty;

        var question = await _db.EssayQuestions.FindAsync(request.QuestionId);
        var questionContent = question?.Content ?? string.Empty;

        var matchedSegments = FindMatchingSegments(textA, textB, NgramSize);
        var similarityPercent = CalculateSimilarity(textA, textB, matchedSegments);

        var studentNameA = attemptA.User?.FullName ?? "Học viên A";
        var studentNameB = attemptB.User?.FullName ?? "Học viên B";

        var aiReport = await GenerateAiReportAsync(
            studentNameA, studentNameB, questionContent, textA, textB, similarityPercent, matchedSegments);

        return new PlagiarismCompareResponse(
            request.AttemptIdA,
            request.AttemptIdB,
            studentNameA,
            studentNameB,
            questionContent,
            textA,
            textB,
            similarityPercent,
            matchedSegments,
            aiReport);
    }

    public async Task<List<PlagiarismScanResponse>> ScanLessonAsync(int lessonId, double threshold = 40.0)
    {
        var attempts = await _db.EssayAttempts
            .Include(a => a.User)
            .Include(a => a.Answers)
            .ThenInclude(ans => ans.Question)
            .Where(a => a.LessonId == lessonId && a.Status == "Submitted")
            .ToListAsync();

        var questions = await _db.EssayAnswers
            .Where(ans => attempts.Select(a => a.Id).Contains(ans.AttemptId))
            .Select(ans => new { ans.QuestionId, ans.Question.Content })
            .Distinct()
            .ToListAsync();

        var result = new List<PlagiarismScanResponse>();

        foreach (var q in questions)
        {
            var answersForQuestion = attempts
                .Select(a => new
                {
                    Attempt = a,
                    Answer = a.Answers.FirstOrDefault(ans => ans.QuestionId == q.QuestionId)
                })
                .Where(x => x.Answer is not null && !string.IsNullOrWhiteSpace(x.Answer.Content))
                .ToList();

            var highSimilarityPairs = new List<PlagiarismSessionPair>();

            for (var i = 0; i < answersForQuestion.Count; i++)
            {
                for (var j = i + 1; j < answersForQuestion.Count; j++)
                {
                    var textA = answersForQuestion[i].Answer!.Content;
                    var textB = answersForQuestion[j].Answer!.Content;
                    var segments = FindMatchingSegments(textA, textB, NgramSize);
                    var similarity = CalculateSimilarity(textA, textB, segments);

                    if (similarity >= threshold)
                    {
                        highSimilarityPairs.Add(new PlagiarismSessionPair(
                            answersForQuestion[i].Attempt.Id,
                            answersForQuestion[j].Attempt.Id,
                            answersForQuestion[i].Attempt.User?.FullName ?? "N/A",
                            answersForQuestion[j].Attempt.User?.FullName ?? "N/A",
                            similarity));
                    }
                }
            }

            if (highSimilarityPairs.Count > 0)
            {
                result.Add(new PlagiarismScanResponse(q.QuestionId, q.Content, highSimilarityPairs));
            }
        }

        return result;
    }

    private static List<MatchedSegment> FindMatchingSegments(string textA, string textB, int ngramSize)
    {
        if (string.IsNullOrWhiteSpace(textA) || string.IsNullOrWhiteSpace(textB))
        {
            return [];
        }

        var wordsA = Tokenize(textA);
        var wordsB = Tokenize(textB);

        var hashesB = BuildRollingHashes(wordsB, ngramSize);

        var result = new List<MatchedSegment>();
        var usedRangesA = new HashSet<int>();
        var usedRangesB = new HashSet<int>();

        for (var i = 0; i <= wordsA.Count - ngramSize; i++)
        {
            if (usedRangesA.Contains(i))
            {
                continue;
            }

            var ngram = string.Join(" ", wordsA.Skip(i).Take(ngramSize));
            var hash = ComputeHash(ngram);

            if (!hashesB.TryGetValue(hash, out var positionsB))
            {
                continue;
            }

            foreach (var posB in positionsB)
            {
                if (usedRangesB.Contains(posB))
                {
                    continue;
                }

                var ngramB = string.Join(" ", wordsB.Skip(posB).Take(ngramSize));
                if (!string.Equals(ngram, ngramB, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var extLen = ngramSize;
                while (i + extLen < wordsA.Count && posB + extLen < wordsB.Count &&
                       string.Equals(wordsA[i + extLen], wordsB[posB + extLen], StringComparison.OrdinalIgnoreCase))
                {
                    extLen++;
                }

                var startCharA = GetCharOffset(textA, wordsA, i);
                var endCharA = GetCharOffset(textA, wordsA, i + extLen - 1) + wordsA[i + extLen - 1].Length;
                var startCharB = GetCharOffset(textB, wordsB, posB);
                var endCharB = GetCharOffset(textB, wordsB, posB + extLen - 1) + wordsB[posB + extLen - 1].Length;
                var matchedText = string.Join(" ", wordsA.Skip(i).Take(extLen));

                result.Add(new MatchedSegment(startCharA, endCharA, startCharB, endCharB, matchedText));

                for (var k = 0; k < extLen; k++)
                {
                    usedRangesA.Add(i + k);
                    usedRangesB.Add(posB + k);
                }
                break;
            }
        }

        return result;
    }

    private static List<string> Tokenize(string text)
    {
        return text
            .Split([' ', '\n', '\r', '\t', '.', ',', '!', '?', ';', ':', '"', '\''], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 1)
            .ToList();
    }

    private static Dictionary<long, List<int>> BuildRollingHashes(List<string> words, int ngramSize)
    {
        var result = new Dictionary<long, List<int>>();
        for (var i = 0; i <= words.Count - ngramSize; i++)
        {
            var ngram = string.Join(" ", words.Skip(i).Take(ngramSize));
            var hash = ComputeHash(ngram);
            if (!result.TryGetValue(hash, out var positions))
            {
                positions = [];
                result[hash] = positions;
            }
            positions.Add(i);
        }
        return result;
    }

    private static long ComputeHash(string s)
    {
        const long Base = 31;
        const long Mod = 1_000_000_007L;
        long hash = 0;
        long power = 1;
        foreach (var ch in s.ToLowerInvariant())
        {
            hash = (hash + ch * power) % Mod;
            power = power * Base % Mod;
        }
        return hash;
    }

    private static int GetCharOffset(string text, List<string> words, int wordIndex)
    {
        var target = words[wordIndex];
        var searchStart = 0;
        for (var i = 0; i < wordIndex; i++)
        {
            var idx = text.IndexOf(words[i], searchStart, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                searchStart = idx + words[i].Length;
            }
        }
        var found = text.IndexOf(target, searchStart, StringComparison.OrdinalIgnoreCase);
        return found >= 0 ? found : searchStart;
    }

    private static double CalculateSimilarity(string textA, string textB, List<MatchedSegment> segments)
    {
        if (string.IsNullOrWhiteSpace(textA) || string.IsNullOrWhiteSpace(textB))
        {
            return 0.0;
        }

        var totalMatchedChars = segments.Sum(s => s.EndA - s.StartA);
        var avgLen = (textA.Length + textB.Length) / 2.0;
        return avgLen > 0 ? Math.Min(100.0, totalMatchedChars * 100.0 / avgLen) : 0.0;
    }

    private async Task<string> GenerateAiReportAsync(
        string studentNameA,
        string studentNameB,
        string questionContent,
        string textA,
        string textB,
        double similarityPercent,
        List<MatchedSegment> segments)
    {
        var highlightedSegments = new StringBuilder();

        foreach (var seg in segments.Take(5))
        {
            highlightedSegments.AppendLine($"- \"{seg.Text}\"");
        }

        var systemPrompt = @"Bạn là Hội đồng Kỷ luật Học thuật (Academic Disciplinary Board) của một tổ chức đào tạo uy tín.
Nhiệm vụ của bạn là xem xét bằng chứng sao chép bài và lập 'Biên bản Ghi nhận Bất thường' một cách chính thức, khách quan và nghiêm túc.";

        var userPrompt = $@"**YÊU CẦU LẬP BIÊN BẢN:**

Câu hỏi đề bài: ""{questionContent}""

Độ tương đồng phát hiện được: **{similarityPercent:F1}%**

Các đoạn văn bản trùng lặp tiêu biểu:
{highlightedSegments}

**Bài làm của Học viên A ({studentNameA}):**
---
{textA.Substring(0, Math.Min(1000, textA.Length))}
---

**Bài làm của Học viên B ({studentNameB}):**
---
{textB.Substring(0, Math.Min(1000, textB.Length))}
---

Hãy soạn thảo 'BIÊN BẢN GHI NHẬN BẤT THƯỜNG' theo cấu trúc sau:

1. **THÔNG TIN VỤ VIỆC**: Ngày lập biên bản, các bên liên quan.
2. **MÔ TẢ BẤT THƯỜNG**: Mô tả chi tiết các đoạn trùng lặp được phát hiện.
3. **PHÂN TÍCH NGỮ NGHĨA**: Dựa trên nội dung, phân tích xem đây là:
   - **Đạo văn có chủ đích**: Nội dung giống nhau ở mức độ sáng tạo, lập luận cá nhân, ví dụ minh họa.
   - **Trùng lặp ngẫu nhiên**: Chỉ giống nhau ở các thuật ngữ chuyên ngành, định nghĩa từ tài liệu chung.
4. **KẾT LUẬN VÀ ĐỀ XUẤT XỬ LÝ**: Mức độ vi phạm (Nhẹ/Trung bình/Nghiêm trọng) và biện pháp xử lý đề xuất.

Viết bằng tiếng Việt, trang trọng, ngắn gọn và chuyên nghiệp.";

        return await _llmService.GenerateResponseAsync(systemPrompt, userPrompt);
    }
}
