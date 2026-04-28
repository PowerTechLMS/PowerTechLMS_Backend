using LMS.Core.DTOs;

namespace LMS.Core.Interfaces;

public interface IPlagiarismService
{
    Task<PlagiarismCompareResponse> CompareAsync(PlagiarismCompareRequest request);

    Task<List<PlagiarismScanResponse>> ScanLessonAsync(int lessonId, double threshold = 40.0);
}
