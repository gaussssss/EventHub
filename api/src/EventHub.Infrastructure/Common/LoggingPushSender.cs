using EventHub.Domain.Services;
using Microsoft.Extensions.Logging;

namespace EventHub.Infrastructure.Common;

/// <summary>
/// Adaptateur push no-op : journalise l'envoi tant que la config du fournisseur
/// (FCM/APNs) n'est pas fournie. Sera remplacé par l'implémentation réelle.
/// </summary>
public sealed class LoggingPushSender : IPushSender
{
    private readonly ILogger<LoggingPushSender> _logger;

    public LoggingPushSender(ILogger<LoggingPushSender> logger) => _logger = logger;

    public Task SendAsync(
        Guid userId, string title, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Push (stub) → user {UserId} : {Title} — {Body}", userId, title, body);
        return Task.CompletedTask;
    }
}
