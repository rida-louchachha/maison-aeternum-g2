using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class Notification : AuditableEntity
{
    public string UserId { get; set; } = default!;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
}
