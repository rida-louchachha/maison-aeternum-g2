using MaisonAeternum.Application.AiMentor.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace MaisonAeternum.Infrastructure.AiMentor;

/// <summary>
/// HeyGen Streaming Avatar integration (https://docs.heygen.com/docs/streaming-avatar-sdk).
///
/// HeyGen's streaming product is a two-hop flow: our server mints a short-lived streaming
/// token with the real (secret) API key, and hands that token to the browser. The browser's
/// own `@heygen/streaming-avatar` SDK then uses the token to open the actual WebRTC session
/// directly with HeyGen — the *real* session id only exists once the browser has connected,
/// so this client cannot return one at CreateSessionAsync time. Once connected, the frontend
/// has HeyGen's true session id and includes it on every subsequent chat call; this client's
/// SendSpeechAsync then drives that live session from the server via HeyGen's
/// `streaming.task` endpoint — this is what lets Aurèle's *generated* reply (built from our
/// own learner data) be what the avatar actually says, rather than some canned line.
/// </summary>
public class HeyGenAvatarClient : IAvatarClient
{
    private readonly HttpClient _httpClient;
    private readonly HeyGenOptions _options;
    private readonly ILogger<HeyGenAvatarClient> _logger;

    public HeyGenAvatarClient(HttpClient httpClient, IOptions<HeyGenOptions> options, ILogger<HeyGenAvatarClient> logger)
    {
        _options = options.Value;
        _logger = logger;

        httpClient.BaseAddress = new Uri(_options.BaseUrl);
        httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient = httpClient;
    }

    public string ProviderName => "HeyGen";

    public bool IsConfigured => _options.IsConfigured;

    public async Task<AvatarSessionResult> CreateSessionAsync(AvatarSessionRequest request, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return AvatarSessionResult.Fail(ProviderName, "HeyGen API key is not configured.");

        var response = await _httpClient.PostAsync("/v1/streaming.create_token", content: null, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("HeyGen streaming.create_token returned {StatusCode}: {Body}", response.StatusCode, body);
            return AvatarSessionResult.Fail(ProviderName, $"HeyGen rejected the token request ({(int)response.StatusCode}).");
        }

        var payload = await response.Content.ReadFromJsonAsync<HeyGenTokenResponse>(cancellationToken: cancellationToken);
        var token = payload?.Data?.Token;

        if (string.IsNullOrEmpty(token))
            return AvatarSessionResult.Fail(ProviderName, "HeyGen did not return a streaming token.");

        var clientPayload = JsonSerializer.Serialize(new
        {
            provider = "heygen",
            token,
            avatarId = _options.AvatarId,
            voiceId = _options.VoiceId,
            quality = _options.Quality
        });

        // No real session id yet — it is minted by HeyGen when the browser SDK connects with this token.
        return new AvatarSessionResult
        {
            Success = true,
            SessionId = null,
            ClientPayloadJson = clientPayload,
            ProviderName = ProviderName
        };
    }

    public async Task<AvatarSpeechResult> SendSpeechAsync(string sessionId, string text, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return AvatarSpeechResult.Fail("HeyGen API key is not configured.");

        var response = await _httpClient.PostAsJsonAsync("/v1/streaming.task", new
        {
            session_id = sessionId,
            text,
            task_type = "repeat" // "repeat" = speak this exact text verbatim (vs. HeyGen's own LLM completion mode)
        }, cancellationToken);

        if (response.IsSuccessStatusCode)
            return AvatarSpeechResult.Ok();

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning("HeyGen streaming.task returned {StatusCode} for session {SessionId}: {Body}", response.StatusCode, sessionId, body);
        return AvatarSpeechResult.Fail($"HeyGen rejected the speech request ({(int)response.StatusCode}).");
    }

    public async Task CloseSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrEmpty(sessionId)) return;

        var response = await _httpClient.PostAsJsonAsync("/v1/streaming.stop", new { session_id = sessionId }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("HeyGen streaming.stop returned {StatusCode} for session {SessionId}", response.StatusCode, sessionId);
        }
    }

    private class HeyGenTokenResponse
    {
        public HeyGenTokenData? Data { get; set; }
    }

    private class HeyGenTokenData
    {
        public string? Token { get; set; }
    }
}
