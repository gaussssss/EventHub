using EventHub.Domain.Services;
using EventHub.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.Api.Realtime;

/// <summary>Implémentation SignalR de <see cref="IRealtimeNotifier"/>.</summary>
public sealed class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hub;

    public SignalRNotifier(IHubContext<NotificationsHub> hub) => _hub = hub;

    public Task ActivityParticipantsChangedAsync(
        Guid activityId, int currentParticipants, CancellationToken cancellationToken = default) =>
        _hub.Clients.All.SendAsync(
            "activityParticipantsChanged",
            new { activityId, currentParticipants },
            cancellationToken);

    public Task RegistrationPromotedAsync(
        Guid userId, Guid activityId, CancellationToken cancellationToken = default) =>
        _hub.Clients.All.SendAsync(
            "registrationPromoted",
            new { userId, activityId },
            cancellationToken);
}
