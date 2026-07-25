using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.Common.Interfaces;

public interface IAiConversationRepository : IRepository<AIConversation>
{
    Task<AIConversation?> FindActiveConversationAsync(
        int learnerId, AiConversationContext context, int? relatedFormationId, int? relatedModuleId, int? relatedQuizAttemptId,
        CancellationToken cancellationToken = default);

    Task<AIConversation?> GetWithMessagesAsync(int conversationId, CancellationToken cancellationToken = default);

    Task<List<AIConversation>> GetRecentByLearnerAsync(int learnerId, int count, CancellationToken cancellationToken = default);
}
