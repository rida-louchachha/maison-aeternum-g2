using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class MediaFile : AuditableEntity
{
    public string FileName { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = default!;
    public MediaType MediaType { get; set; }
    public string? UploadedByUserId { get; set; }
}
