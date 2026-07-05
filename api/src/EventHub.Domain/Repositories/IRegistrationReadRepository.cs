using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Lecture des inscriptions d'une activité (back office).</summary>
public interface IRegistrationReadRepository
{
    /// <summary>Inscrits + liste d'attente (statuts non annulés), avec l'identité.</summary>
    Task<IReadOnlyList<RegistrationEntryDto>> GetByActivityAsync(
        Guid activityId, CancellationToken cancellationToken = default);
}
