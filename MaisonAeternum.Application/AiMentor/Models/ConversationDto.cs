using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Application.AiMentor.Models;

public class ConversationDto
{
    public int Id { get; set; }
    public AiConversationContext Context { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsActive { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = new();
}

public class ConversationSummaryDto
{
    public int Id { get; set; }
    public AiConversationContext Context { get; set; }
    public string? Preview { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int MessageCount { get; set; }
}

public class ChatMessageDto
{
    public int Id { get; set; }
    public MessageSender Sender { get; set; }
    public string Text { get; set; } = default!;
    public DateTime SentAt { get; set; }
}
