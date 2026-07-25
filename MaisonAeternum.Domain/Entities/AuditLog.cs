using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
