namespace MaisonAeternum.Domain.Entities;

public class LearnerBadge
{
    public int LearnerId { get; set; }
    public int BadgeId { get; set; }
    public DateTime EarnedAt { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public Badge Badge { get; set; } = default!;
}
