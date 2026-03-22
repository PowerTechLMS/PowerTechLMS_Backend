
namespace LMS.Core.Interfaces;

public interface IAiProcessingService
{
    Task ProcessLessonVideoAsync(int lessonId);

    Task ProcessDocumentAsync(int documentId);

    Task ProcessLessonAttachmentAsync(int attachmentId);
}
