using MaisonAeternum.Application.AiMentor.Abstractions;
using Microsoft.Extensions.Logging;

namespace MaisonAeternum.Infrastructure.AiMentor;

/// <summary>
/// Decorator around whichever real <see cref="IAvatarClient"/> is registered (HeyGen or
/// Anam). If the provider is not configured (no API key) or throws/fails at call time, this
/// transparently serves <see cref="MockAvatarClient"/> instead — the one place that satisfies
/// "the application must never crash because of the AI provider" and "automatically use
/// MockAvatarClient when appropriate," without scattering try/catch-and-fallback logic across
/// every call site.
/// </summary>
public class FallbackAvatarClient : IAvatarClient
{
    private readonly IAvatarClient _primary;
    private readonly MockAvatarClient _fallback;
    private readonly ILogger<FallbackAvatarClient> _logger;

    public FallbackAvatarClient(IAvatarClient primary, MockAvatarClient fallback, ILogger<FallbackAvatarClient> logger)
    {
        _primary = primary;
        _fallback = fallback;
        _logger = logger;
    }

    public string ProviderName => _primary.IsConfigured ? _primary.ProviderName : _fallback.ProviderName;

    public bool IsConfigured => true; // there is always at least the mock to fall back to

    public async Task<AvatarSessionResult> CreateSessionAsync(AvatarSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (!_primary.IsConfigured)
        {
            _logger.LogInformation("{Provider} has no API key configured — using MockAvatarClient.", _primary.ProviderName);
            return await _fallback.CreateSessionAsync(request, cancellationToken);
        }

        try
        {
            var result = await _primary.CreateSessionAsync(request, cancellationToken);
            if (result.Success) return result;

            _logger.LogWarning("{Provider} failed to create a session ({Error}) — falling back to MockAvatarClient.",
                _primary.ProviderName, result.ErrorMessage);
            return await _fallback.CreateSessionAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Provider} threw while creating a session — falling back to MockAvatarClient.", _primary.ProviderName);
            return await _fallback.CreateSessionAsync(request, cancellationToken);
        }
    }

    public async Task<AvatarSpeechResult> SendSpeechAsync(string sessionId, string text, CancellationToken cancellationToken = default)
    {
        // Mock session ids are self-identifying (prefixed) — once a session has fallen back,
        // every subsequent call for it must also go to the mock, or the real provider will
        // reject an unrecognized session id.
        if (!_primary.IsConfigured || sessionId.StartsWith("mock-", StringComparison.Ordinal))
        {
            return await _fallback.SendSpeechAsync(sessionId, text, cancellationToken);
        }

        try
        {
            return await _primary.SendSpeechAsync(sessionId, text, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Provider} threw while sending speech for session {SessionId}.", _primary.ProviderName, sessionId);
            return AvatarSpeechResult.Fail("The avatar provider is temporarily unavailable.");
        }
    }

    public async Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (!_primary.IsConfigured || sessionId.StartsWith("mock-", StringComparison.Ordinal))
        {
            await _fallback.CloseSessionAsync(sessionId, cancellationToken);
            return;
        }

        try
        {
            await _primary.CloseSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{Provider} threw while closing session {SessionId} — ignoring.", _primary.ProviderName, sessionId);
        }
    }
}
