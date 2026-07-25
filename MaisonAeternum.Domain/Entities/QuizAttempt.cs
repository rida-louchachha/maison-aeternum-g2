using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class QuizAttempt : AuditableEntity
{
    public int QuizId { get; set; }
    public int LearnerId { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal ScorePercentage { get; set; }
    public bool Passed { get; set; }
    public int TimeTakenSeconds { get; set; }

    public Quiz Quiz { get; set; } = default!;
    public LearnerProfile Learner { get; set; } = default!;
    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
    public Certificate? Certificate { get; set; }
}
