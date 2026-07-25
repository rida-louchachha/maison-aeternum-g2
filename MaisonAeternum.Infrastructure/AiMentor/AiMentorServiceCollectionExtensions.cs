using MaisonAeternum.Application.AiMentor.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MaisonAeternum.Infrastructure.AiMentor;

public static class AiMentorServiceCollectionExtensions
{
    public static IServiceCollection AddAiMentor(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HeyGenOptions>(configuration.GetSection(HeyGenOptions.SectionName));
        services.Configure<AnamOptions>(configuration.GetSection(AnamOptions.SectionName));

        services.AddHttpClient<HeyGenAvatarClient>();
        services.AddHttpClient<AnamAvatarClient>();
        services.AddSingleton<MockAvatarClient>();

        // ── Avatar provider switch ──────────────────────────────────────────────
        // HeyGen is the default. To switch to Anam.ai, change ONLY the generic type
        // argument on the line below — nothing else in the application depends on
        // which provider is selected here, since everything above talks to
        // IAvatarClient (and, above that, only ever to IAiMentorService).
        //
        // Whichever provider is chosen is automatically wrapped in a
        // FallbackAvatarClient: if its API key is missing from configuration, or a
        // call to it fails at runtime, requests are transparently served by
        // MockAvatarClient instead — the app never crashes or blocks on the AI
        // provider being unavailable.
        services.AddScoped<IAvatarClient>(sp => BuildAvatarClient<HeyGenAvatarClient>(sp));
        // services.AddScoped<IAvatarClient>(sp => BuildAvatarClient<AnamAvatarClient>(sp)); // ← switch to Anam.ai

        return services;
    }

    private static IAvatarClient BuildAvatarClient<TPrimary>(IServiceProvider serviceProvider) where TPrimary : class, IAvatarClient
    {
        var primary = serviceProvider.GetRequiredService<TPrimary>();
        var mock = serviceProvider.GetRequiredService<MockAvatarClient>();
        var logger = serviceProvider.GetRequiredService<ILogger<FallbackAvatarClient>>();
        return new FallbackAvatarClient(primary, mock, logger);
    }
}
