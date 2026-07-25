using MaisonAeternum.Application.Common.Interfaces;
using MaisonAeternum.Domain.Entities;
using MaisonAeternum.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MaisonAeternum.Infrastructure.Services;

/// <summary>
/// Development-mode email sender: persists the message to the SentEmail table (viewable
/// under /Admin/SentEmails) instead of requiring real SMTP credentials for the demo/defense.
/// </summary>
public class DevEmailSender : IEmailSender
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DevEmailSender> _logger;

    public DevEmailSender(ApplicationDbContext context, ILogger<DevEmailSender> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _context.SentEmails.Add(new SentEmail
        {
            To = to,
            Subject = subject,
            Body = htmlBody,
            SentAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("[DevEmailSender] Simulated email to {To}: {Subject}", to, subject);
    }
}
