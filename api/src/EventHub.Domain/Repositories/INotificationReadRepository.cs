using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Lecture des notifications de l'utilisateur courant.</summary>
public interface INotificationReadRepository
{
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId, CancellationToken cancellationToken = default);
}
