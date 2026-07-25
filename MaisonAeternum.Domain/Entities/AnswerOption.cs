using MaisonAeternum.Domain.Common;

namespace MaisonAeternum.Domain.Entities;

public class AnswerOption : AuditableEntity
{
    public int QuestionId { get; set; }
    public string Text { get; set; } = default!;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }

    public Question Question { get; set; } = default!;
}
