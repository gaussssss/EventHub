using EventHub.Domain.ReadModels;
namespace EventHub.Domain.Repositories;

/// <summary>Lecture de l'annuaire des utilisateurs (recherche back office).</summary>
public interface IUserAdminReadRepository
{
    Task<IReadOnlyList<AdminUserDto>> SearchAsync(
        string? query, CancellationToken cancellationToken = default);
}
