using MaisonAeternum.Application.AiMentor.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MaisonAeternum.Infrastructure.AiMentor;

/// <summary>
/// Default client when no real avatar provider is configured (missing API key, or explicit
/// "Mock" selection for local dev / grading without burning API quota). Never fails, never
/// calls out to the network — the frontend detects `provider: "mock"` in the session payload
/// and falls back to a static Aurèle illustration plus the browser's own SpeechSynthesis API
/// for voice, so the assistant still genuinely talks, just without a photoreal avatar.
/// </summary>
public class MockAvatarClient : IAvatarClient
{
    private readonly ILogger<MockAvatarClient> _logger;

    public MockAvatarClient(ILogger<MockAvatarClient> logger)
    {
        _logger = logger;
    }

    public string ProviderName => "Mock";

    public bool IsConfigured => true;

    public Task<AvatarSessionResult> CreateSessionAsync(AvatarSessionRequest request, CancellationToken cancellationToken = default)
    {
        var sessionId = $"mock-{Guid.NewGuid():N}";
        _logger.LogInformation("MockAvatarClient: opened simulated session {SessionId} for {Learner}", sessionId, request.LearnerDisplayName);

        var payload = JsonSerializer.Serialize(new
        {
            provider = "mock",
            mode = "browser-speech-synthesis",
            avatarImage = "/img/aurele-avatar.svg"
        });

        return Task.FromResult(new AvatarSessionResult
        {
            Success = true,
            SessionId = sessionId,
            ClientPayloadJson = payload,
            ProviderName = ProviderName
        });
    }

    public Task<AvatarSpeechResult> SendSpeechAsync(string sessionId, string text, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockAvatarClient: session {SessionId} would speak: {Text}", sessionId, text);
        // The mock frontend widget reads the returned text aloud itself via speechSynthesis —
        // there is no server-side rendering to relay, so this is handled client-side by design.
        return Task.FromResult(AvatarSpeechResult.Ok(handledClientSide: true));
    }

    public Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("MockAvatarClient: closed simulated session {SessionId}", sessionId);
        return Task.CompletedTask;
    }
}
