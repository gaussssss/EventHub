using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Hearts;
using EventHub.Domain.Entities;

namespace EventHub.Application.Users;

/// <summary>
/// Ajustement manuel de cœurs par un admin (POST /api/admin/users/{id}/hearts).
/// Enregistré comme transaction <c>admin_adjust</c> ; le montant peut être négatif.
/// </summary>
public sealed class AdjustHeartsHandler
    : ICommandHandler<AdjustHeartsCommand, AdjustHeartsResult>
{
    private readonly IUserAdminService _users;
    private readonly IHeartTransactionRepository _transactions;
    private readonly IHeartReadRepository _read;
    private readonly IClock _clock;

    public AdjustHeartsHandler(
        IUserAdminService users,
        IHeartTransactionRepository transactions,
        IHeartReadRepository read,
        IClock clock)
    {
        _users = users;
        _transactions = transactions;
        _read = read;
        _clock = clock;
    }

    public async Task<AdjustHeartsResult> HandleAsync(
        AdjustHeartsCommand command, CancellationToken cancellationToken = default)
    {
        var (userId, hearts, reason) = command;
        if (hearts == 0)
            return new AdjustHeartsResult(AdjustHeartsStatus.InvalidAmount, 0);

        if (!await _users.ExistsAsync(userId, cancellationToken))
            return new AdjustHeartsResult(AdjustHeartsStatus.UserNotFound, 0);

        await _transactions.AddAsync(HeartTransaction.ForAdjustment(
            userId, hearts,
            string.IsNullOrWhiteSpace(reason) ? HeartTransaction.ReasonAdjustment : reason,
            _clock.UtcNow), cancellationToken);
        await _transactions.SaveChangesAsync(cancellationToken);

        var total = await _read.GetTotalAsync(userId, cancellationToken);
        return new AdjustHeartsResult(AdjustHeartsStatus.Adjusted, total);
    }
}
