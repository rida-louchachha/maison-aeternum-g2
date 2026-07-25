using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class QuizAttemptAnswer : AuditableEntity
{
    public int QuizAttemptId { get; set; }
    public int QuestionId { get; set; }
    public bool IsCorrect { get; set; }
    public int PointsAwarded { get; set; }

    public QuizAttempt QuizAttempt { get; set; } = default!;
    public Question Question { get; set; } = default!;
    public ICollection<QuizAttemptSelectedOption> SelectedOptions { get; set; } = new List<QuizAttemptSelectedOption>();
}
