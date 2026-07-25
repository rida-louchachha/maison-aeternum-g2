namespace MaisonAeternum.Application.AiMentor.Models;

public class AvatarSessionDto
{
    public bool Success { get; set; }
    public string? SessionId { get; set; }
    public string ProviderName { get; set; } = default!;
    /// <summary>Opaque provider payload (token/ICE servers/etc.), passed straight through to the frontend SDK.</summary>
    public string? ClientPayloadJson { get; set; }
    public string? ErrorMessage { get; set; }
}
