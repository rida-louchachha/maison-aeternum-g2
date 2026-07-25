using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class Enrollment : AuditableEntity
{
    public int LearnerId { get; set; }
    public int FormationId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public EnrollmentStatus Status { get; set; }
    public decimal ProgressPercentage { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public Formation Formation { get; set; } = default!;
    public ICollection<ModuleProgress> ModuleProgresses { get; set; } = new List<ModuleProgress>();
}
