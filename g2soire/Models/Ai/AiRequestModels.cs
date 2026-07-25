using System.ComponentModel.DataAnnotations;

namespace MaisonAeternum.Web.Models.Ai;

public class SessionScopedRequest
{
    public string? SessionId { get; set; }
}

public class AskRequest
{
    [Required(ErrorMessage = "Please type a question.")]
    [StringLength(1000, ErrorMessage = "Keep questions under 1000 characters.")]
    public string Question { get; set; } = default!;

    public int? ConversationId { get; set; }

    public string? SessionId { get; set; }
}
