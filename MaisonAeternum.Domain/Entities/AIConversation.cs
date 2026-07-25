using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class AIConversation : AuditableEntity
{
    public int LearnerId { get; set; }
    public AiConversationContext Context { get; set; }
    public int? RelatedFormationId { get; set; }
    public int? RelatedModuleId { get; set; }
    public int? RelatedQuizAttemptId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsActive { get; set; }

    public LearnerProfile Learner { get; set; } = default!;
    public Formation? RelatedFormation { get; set; }
    public Module? RelatedModule { get; set; }
    public QuizAttempt? RelatedQuizAttempt { get; set; }
    public ICollection<AIMessage> Messages { get; set; } = new List<AIMessage>();
}
