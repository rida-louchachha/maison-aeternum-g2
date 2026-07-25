using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class ModuleProgress : AuditableEntity
{
    public int EnrollmentId { get; set; }
    public int ModuleId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TimeSpentMinutes { get; set; }

    public Enrollment Enrollment { get; set; } = default!;
    public Module Module { get; set; } = default!;
}
