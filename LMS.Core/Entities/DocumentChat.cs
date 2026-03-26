
namespace LMS.Core.Entities;

public class DocumentChat : BaseEntity
{
    public int Id { get; set; }

    public int DocumentId { get; set; }

    public Document? Document { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public string UserMessage { get; set; } = string.Empty;

    public string AiResponse { get; set; } = string.Empty;
}
