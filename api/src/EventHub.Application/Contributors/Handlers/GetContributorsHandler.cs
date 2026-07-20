using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Contributors;

public sealed class GetContributorsHandler
    : IQueryHandler<GetContributorsQuery, IReadOnlyList<ContributorDto>>
{
    private readonly IContributorReadRepository _contributors;

    public GetContributorsHandler(IContributorReadRepository contributors) =>
        _contributors = contributors;

    public Task<IReadOnlyList<ContributorDto>> HandleAsync(
        GetContributorsQuery query, CancellationToken cancellationToken = default) =>
        _contributors.GetAllAsync(cancellationToken);
}
