using MaisonAeternum.Domain.Common;
using MaisonAeternum.Domain.Enums;

namespace MaisonAeternum.Domain.Entities;

public class AIMessage : AuditableEntity
{
    public int ConversationId { get; set; }
    public MessageSender Sender { get; set; }
    public string MessageText { get; set; } = default!;
    public int? AudioMediaFileId { get; set; }

    public AIConversation Conversation { get; set; } = default!;
    public MediaFile? AudioMediaFile { get; set; }
}
