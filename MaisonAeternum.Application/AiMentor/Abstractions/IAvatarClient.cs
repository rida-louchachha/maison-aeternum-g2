namespace MaisonAeternum.Application.AiMentor.Abstractions;

/// <summary>
/// Provider-agnostic contract for a real-time talking avatar (HeyGen, Anam.ai, or an
/// equivalent streaming-avatar service). Implementations own all HTTP/auth details for
/// their provider; nothing above this interface knows which provider is in use.
///
/// Real streaming-avatar providers are session-based: the server creates a session and
/// hands the browser a short-lived token/credential set, then the browser's own SDK opens
/// a WebRTC connection directly to the provider and renders the video. This interface
/// reflects that shape — it does not (and cannot) return raw video/audio bytes itself.
/// </summary>
public interface IAvatarClient
{
    /// <summary>Machine-readable provider name, e.g. "HeyGen", "Anam", "Mock" — surfaced to the frontend so it knows which SDK to load.</summary>
    string ProviderName { get; }

    /// <summary>True when this client has the configuration (API key, persona/avatar id) needed to actually call the provider.</summary>
    bool IsConfigured { get; }

    /// <summary>Opens a new avatar session for a learner and returns the credentials the frontend SDK needs to connect.</summary>
    Task<AvatarSessionResult> CreateSessionAsync(AvatarSessionRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asks an already-open session to speak the given text. Some providers (HeyGen) support
    /// this server-side "task" call; others (Anam, depending on SDK version) expect the
    /// browser to call `.talk()` directly using the session token — in that case the
    /// implementation returns Success with <see cref="AvatarSpeechResult.HandledClientSide"/> = true
    /// so the frontend knows to speak the text itself instead of waiting on the server.
    /// </summary>
    Task<AvatarSpeechResult> SendSpeechAsync(string sessionId, string text, CancellationToken cancellationToken = default);

    /// <summary>Closes a session and releases the provider-side resources.</summary>
    Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default);
}

public class AvatarSessionRequest
{
    public required string LearnerDisplayName { get; init; }
}

public class AvatarSessionResult
{
    public bool Success { get; init; }
    public string? SessionId { get; init; }
    /// <summary>Opaque JSON blob (token, ICE servers, ws url, whatever the provider's SDK needs) — passed through untouched to the frontend.</summary>
    public string? ClientPayloadJson { get; init; }
    public string ProviderName { get; init; } = default!;
    public string? ErrorMessage { get; init; }

    public static AvatarSessionResult Fail(string providerName, string error) =>
        new() { Success = false, ProviderName = providerName, ErrorMessage = error };
}

public class AvatarSpeechResult
{
    public bool Success { get; init; }
    public bool HandledClientSide { get; init; }
    public string? ErrorMessage { get; init; }

    public static AvatarSpeechResult Ok(bool handledClientSide = false) => new() { Success = true, HandledClientSide = handledClientSide };
    public static AvatarSpeechResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
