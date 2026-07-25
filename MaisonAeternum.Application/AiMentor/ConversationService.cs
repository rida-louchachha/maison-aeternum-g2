using MaisonAeternum.Application.AiMentor.Abstractions;
using MaisonAeternum.Application.AiMentor.Models;
using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.AiMentor;

public class ConversationService : IConversationService
{
    private readonly IAiConversationRepository _conversations;
    private readonly IRepository<AIMessage> _messages;

    public ConversationService(IAiConversationRepository conversations, IRepository<AIMessage> messages)
    {
        _conversations = conversations;
        _messages = messages;
    }

    public async Task<int> GetOrStartConversationAsync(
        int learnerId, AiConversationContext context, int? relatedFormationId, int? relatedModuleId, int? relatedQuizAttemptId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _conversations.FindActiveConversationAsync(
            learnerId, context, relatedFormationId, relatedModuleId, relatedQuizAttemptId, cancellationToken);

        if (existing is not null) return existing.Id;

        var now = DateTime.UtcNow;
        var conversation = new AIConversation
        {
            LearnerId = learnerId,
            Context = context,
            RelatedFormationId = relatedFormationId,
            RelatedModuleId = relatedModuleId,
            RelatedQuizAttemptId = relatedQuizAttemptId,
            StartedAt = now,
            LastMessageAt = now,
            IsActive = true
        };

        await _conversations.AddAsync(conversation, cancellationToken);
        await _conversations.SaveChangesAsync(cancellationToken);
        return conversation.Id;
    }

    public async Task<int> AppendMessageAsync(int conversationId, MessageSender sender, string text, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversations.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new InvalidOperationException($"AIConversation {conversationId} not found.");

        var message = new AIMessage
        {
            ConversationId = conversationId,
            Sender = sender,
            MessageText = text
        };

        await _messages.AddAsync(message, cancellationToken);

        conversation.LastMessageAt = DateTime.UtcNow;
        _conversations.Update(conversation);

        await _conversations.SaveChangesAsync(cancellationToken);
        return message.Id;
    }

    public async Task<ConversationDto?> GetConversationAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversations.GetWithMessagesAsync(conversationId, cancellationToken);
        return conversation is null ? null : MapToDto(conversation);
    }

    public async Task<List<ConversationSummaryDto>> GetRecentConversationsAsync(int learnerId, int count, CancellationToken cancellationToken = default)
    {
        var conversations = await _conversations.GetRecentByLearnerAsync(learnerId, count, cancellationToken);

        return conversations.Select(c => new ConversationSummaryDto
        {
            Id = c.Id,
            Context = c.Context,
            Preview = c.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()?.MessageText,
            LastMessageAt = c.LastMessageAt,
            MessageCount = c.Messages.Count
        }).ToList();
    }

    public async Task CloseConversationAsync(int conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversations.GetByIdAsync(conversationId, cancellationToken);
        if (conversation is null) return;

        conversation.IsActive = false;
        _conversations.Update(conversation);
        await _conversations.SaveChangesAsync(cancellationToken);
    }

    private static ConversationDto MapToDto(AIConversation conversation) => new()
    {
        Id = conversation.Id,
        Context = conversation.Context,
        StartedAt = conversation.StartedAt,
        LastMessageAt = conversation.LastMessageAt,
        IsActive = conversation.IsActive,
        Messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto { Id = m.Id, Sender = m.Sender, Text = m.MessageText, SentAt = m.CreatedAt })
            .ToList()
    };
}
