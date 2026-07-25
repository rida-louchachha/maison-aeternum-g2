using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class ContentBookmark : AuditableEntity
{
    public int LearnerId { get; set; }
    public int ContentItemId { get; set; }
    public string? Note { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public ContentItem ContentItem { get; set; } = default!;
}
