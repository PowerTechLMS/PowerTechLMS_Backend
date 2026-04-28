namespace LMS.Core.DTOs;

public record PlagiarismCompareRequest(int AttemptIdA, int AttemptIdB, int QuestionId);

public record PlagiarismCompareResponse(
    int AttemptIdA,
    int AttemptIdB,
    string StudentNameA,
    string StudentNameB,
    string QuestionContent,
    string TextA,
    string TextB,
    double SimilarityPercent,
    List<MatchedSegment> MatchedSegments,
    string AiReport);

public record MatchedSegment(int StartA, int EndA, int StartB, int EndB, string Text);

public record PlagiarismSessionPair(
    int AttemptIdA,
    int AttemptIdB,
    string StudentNameA,
    string StudentNameB,
    double SimilarityPercent);

public record PlagiarismScanResponse(
    int QuestionId,
    string QuestionContent,
    List<PlagiarismSessionPair> HighSimilarityPairs);
