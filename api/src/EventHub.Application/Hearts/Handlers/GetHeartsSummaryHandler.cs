using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Domain.ValueObjects;

namespace EventHub.Application.Hearts;

/// <summary>Compose le résumé de cœurs : total + niveau (domaine) + historique.</summary>
public sealed class GetHeartsSummaryHandler
    : IQueryHandler<GetHeartsSummaryQuery, HeartsSummaryDto>
{
    private readonly IHeartReadRepository _hearts;

    public GetHeartsSummaryHandler(IHeartReadRepository hearts) => _hearts = hearts;

    public async Task<HeartsSummaryDto> HandleAsync(
        GetHeartsSummaryQuery query, CancellationToken cancellationToken = default)
    {
        var userId = query.UserId;
        var total = await _hearts.GetTotalAsync(userId, cancellationToken);
        var history = await _hearts.GetHistoryAsync(userId, cancellationToken);
        var level = HeartLevel.FromHearts(total);

        return new HeartsSummaryDto
        {
            TotalHearts = total,
            Level = level.Name,
            PreviousThreshold = level.PreviousThreshold,
            NextThreshold = level.NextThreshold,
            History = history,
        };
    }
}
