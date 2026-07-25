using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class Question : AuditableEntity
{
    public int QuizId { get; set; }
    public string Text { get; set; } = default!;
    public QuestionType Type { get; set; }
    public string? ImageUrl { get; set; }
    public string Explanation { get; set; } = default!;
    public int Points { get; set; }
    public int DisplayOrder { get; set; }

    public Quiz Quiz { get; set; } = default!;
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
    public ICollection<QuizAttemptAnswer> AttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
