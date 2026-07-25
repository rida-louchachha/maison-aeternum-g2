namespace MaisonAeternum.Infrastructure.AiMentor;

/// <summary>Bound from configuration section "Ai:HeyGen". In production, ApiKey comes from the
/// Railway environment variable Ai__HeyGen__ApiKey — never hardcoded, never committed.</summary>
public class HeyGenOptions
{
    public const string SectionName = "Ai:HeyGen";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.heygen.com";
    public string AvatarId { get; set; } = "Marianne_Chair_Sitting_public";
    public string VoiceId { get; set; } = string.Empty;
    public string Quality { get; set; } = "medium";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);
}
