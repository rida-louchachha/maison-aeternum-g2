using MaisonAeternum.Application.AiMentor.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaisonAeternum.Infrastructure.AiMentor;

/// <summary>
/// Anam.ai integration (https://docs.anam.ai). Anam's session model is a single server call:
/// exchange the (secret) API key for a short-lived session token scoped to a persona, hand
/// that token to the browser, and the `@anam-ai/js-sdk` client opens the WebRTC session and
/// renders it directly — there is no separate "real session id" round trip like HeyGen's.
///
/// Unlike HeyGen, Anam's public API does not expose a server-side "make this open session
/// speak X" endpoint — driving speech is a client-side SDK call (`client.talk(text)`) against
/// the token-authenticated session. So SendSpeechAsync here does not call out to Anam at all;
/// it reports <see cref="AvatarSpeechResult.HandledClientSide"/> = true, and the frontend
/// widget calls `.talk(text)` itself with the text this server generated. This asymmetry
/// between providers is real, not an oversight — see the AI Trainer docs for the full
/// provider comparison.
/// </summary>
public class AnamAvatarClient : IAvatarClient
{
    private readonly HttpClient _httpClient;
    private readonly AnamOptions _options;
    private readonly ILogger<AnamAvatarClient> _logger;

    public AnamAvatarClient(HttpClient httpClient, IOptions<AnamOptions> options, ILogger<AnamAvatarClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        httpClient.BaseAddress = new Uri(_options.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient = httpClient;
    }

    public string ProviderName => "Anam";

    public bool IsConfigured => _options.IsConfigured;

    public async Task<AvatarSessionResult> CreateSessionAsync(AvatarSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return AvatarSessionResult.Fail(ProviderName, "Anam API key is not configured.");

        var response = await _httpClient.PostAsJsonAsync("/v1/auth/session-token", new
        {
            personaConfig = new
            {
                name = _options.PersonaName,
                avatarId = _options.AvatarId,
                voiceId = _options.VoiceId,
                systemPrompt = _options.SystemPrompt
            }
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Anam auth/session-token returned {StatusCode}: {Body}", response.StatusCode, body);
            return AvatarSessionResult.Fail(ProviderName, $"Anam rejected the session request ({(int)response.StatusCode}).");
        }

        var payload = await response.Content.ReadFromJsonAsync<AnamSessionResponse>(cancellationToken: cancellationToken);

        if (string.IsNullOrEmpty(payload?.SessionToken))
            return AvatarSessionResult.Fail(ProviderName, "Anam did not return a session token.");

        var clientPayload = JsonSerializer.Serialize(new
        {
            provider = "anam",
            sessionToken = payload.SessionToken
        });

        return new AvatarSessionResult
        {
            Success = true,
            SessionId = payload.SessionToken, // Anam has no separate session id — the token itself identifies the session.
            ClientPayloadJson = clientPayload,
            ProviderName = ProviderName
        };
    }

    public Task<AvatarSpeechResult> SendSpeechAsync(string sessionId, string text, CancellationToken cancellationToken = default)
    {
        // No server-side "speak" endpoint in Anam's public API — the frontend SDK calls
        // client.talk(text) directly against the session token. See class remarks.
        return Task.FromResult(AvatarSpeechResult.Ok(handledClientSide: true));
    }

    public Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        // Anam session tokens are short-lived and self-expire; no explicit close endpoint to call.
        return Task.CompletedTask;
    }

    private class AnamSessionResponse
    {
        public string? SessionToken { get; set; }
    }
}
