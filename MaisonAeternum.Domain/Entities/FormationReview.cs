using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class FormationReview : AuditableEntity
{
    public int LearnerId { get; set; }
    public int FormationId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = default!;
    public bool IsApproved { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public Formation Formation { get; set; } = default!;
}
