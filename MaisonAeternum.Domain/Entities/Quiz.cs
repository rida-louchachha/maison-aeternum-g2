using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class Quiz : AuditableEntity
{
    public int FormationId { get; set; }
    public int? ModuleId { get; set; }
    public QuizType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Instructions { get; set; } = default!;
    public int? TimeLimitSeconds { get; set; }
    public decimal PassingScore { get; set; }
    public int? MaxAttempts { get; set; }
    public bool RandomizeQuestions { get; set; }
    public int? QuestionsToServe { get; set; }

    public Formation Formation { get; set; } = default!;
    public Module? Module { get; set; }
    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
