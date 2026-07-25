using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class ActivityLog : AuditableEntity
{
    public int LearnerId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public int MinutesSpent { get; set; }
    public int ModulesCompletedCount { get; set; }
    public int QuizAttemptsCount { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
}
