namespace MaisonAeternum.Application.AiMentor.Models;

public class AiReplyDto
{
    public int ConversationId { get; set; }
    public string Text { get; set; } = default!;
    public bool SpokenByAvatar { get; set; }
    public bool HandledClientSide { get; set; }
    public string? Warning { get; set; }
}
