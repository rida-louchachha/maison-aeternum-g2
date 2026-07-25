using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class ContentItem : AuditableEntity
{
    public int ModuleId { get; set; }
    public ContentItemType Type { get; set; }
    public string Title { get; set; } = default!;
    public int? MediaFileId { get; set; }
    public string? ExternalUrl { get; set; }
    public int DurationMinutes { get; set; }
    public int DisplayOrder { get; set; }

    public Module Module { get; set; } = default!;
    public MediaFile? MediaFile { get; set; }
    public ICollection<ContentBookmark> Bookmarks { get; set; } = new List<ContentBookmark>();
}
