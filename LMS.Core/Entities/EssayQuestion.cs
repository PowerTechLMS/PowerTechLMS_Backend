using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Core.Entities;

public class EssayQuestion : BaseEntity
{
    public int Id { get; set; }

    public int EssayConfigId { get; set; }

    public string Content { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public int Weight { get; set; } = 10;

    public string? ScoringCriteria { get; set; }

    [ForeignKey("EssayConfigId")]
    public virtual EssayConfig EssayConfig { get; set; } = null!;

    public virtual ICollection<EssayAnswer> Answers { get; set; } = new List<EssayAnswer>();
}
