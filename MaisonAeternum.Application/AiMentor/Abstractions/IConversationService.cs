using MaisonAeternum.Application.AiMentor.Models;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.AiMentor.Abstractions;

/// <summary>
/// Pure persistence for AIConversation/AIMessage. Knows nothing about avatars or response
/// generation — <see cref="IAiMentorService"/> is the only caller, keeping this a clean
/// Single-Responsibility component that Application/AiMentor tests can exercise directly.
/// </summary>
public interface IConversationService
{
    /// <summary>Returns the learner's current open conversation for this context, or starts a new one.</summary>
    Task<int> GetOrStartConversationAsync(
        int learnerId, AiConversationContext context, int? relatedFormationId, int? relatedModuleId, int? relatedQuizAttemptId,
        CancellationToken cancellationToken = default);

    Task<int> AppendMessageAsync(int conversationId, MessageSender sender, string text, CancellationToken cancellationToken = default);

    Task<ConversationDto?> GetConversationAsync(int conversationId, CancellationToken cancellationToken = default);

    Task<List<ConversationSummaryDto>> GetRecentConversationsAsync(int learnerId, int count, CancellationToken cancellationToken = default);

    Task CloseConversationAsync(int conversationId, CancellationToken cancellationToken = default);
}
