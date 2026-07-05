namespace EventHub.Domain.Services;

/// <summary>
/// Diffusion temps réel (implémentée par SignalR côté Api). Abstraite ici pour
/// que la couche Application ne dépende pas du transport.
/// </summary>
public interface IRealtimeNotifier
{
    /// <summary>Notifie que le nombre d'inscrits d'une activité a changé.</summary>
    Task ActivityParticipantsChangedAsync(
        Guid activityId, int currentParticipants, CancellationToken cancellationToken = default);

    /// <summary>Notifie un utilisateur qu'il est promu de la liste d'attente à inscrit.</summary>
    Task RegistrationPromotedAsync(
        Guid userId, Guid activityId, CancellationToken cancellationToken = default);
}
