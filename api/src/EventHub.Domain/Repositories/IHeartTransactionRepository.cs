using EventHub.Domain.Entities;

namespace EventHub.Domain.Repositories;

public interface IHeartTransactionRepository
{
    /// <summary>Vrai si les cœurs de présence ont déjà été crédités (idempotence).</summary>
    Task<bool> HasAttendanceCreditAsync(
        Guid userId, Guid activityId, CancellationToken cancellationToken = default);

    Task AddAsync(HeartTransaction transaction, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
