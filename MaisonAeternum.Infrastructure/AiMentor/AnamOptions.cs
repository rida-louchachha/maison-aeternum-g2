namespace MaisonAeternum.Infrastructure.AiMentor;

/// <summary>Bound from configuration section "Ai:Anam". In production, ApiKey comes from the
/// Railway environment variable Ai__Anam__ApiKey — never hardcoded, never committed.</summary>
public class AnamOptions
{
    public const string SectionName = "Ai:Anam";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.anam.ai";
    public string AvatarId { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
    public string PersonaName { get; set; } = "Aurele";
    public string SystemPrompt { get; set; } =
        "You are Aurele, the AI mentor of Maison Aeternum, a watchmaking guild. Speak warmly, briefly, and precisely.";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
