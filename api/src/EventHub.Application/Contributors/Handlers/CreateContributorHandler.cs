using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Contributors;

public sealed class CreateContributorHandler
    : ICommandHandler<CreateContributorCommand, ContributorMutationResult>
{
    private readonly IContributorRepository _contributors;

    public CreateContributorHandler(IContributorRepository contributors) =>
        _contributors = contributors;

    public async Task<ContributorMutationResult> HandleAsync(
        CreateContributorCommand command, CancellationToken cancellationToken = default)
    {
        var contributor = Contributor.Create(
            command.Name, command.Role, command.AvatarUrl, command.SortOrder);
        await _contributors.AddAsync(contributor, cancellationToken);
        await _contributors.SaveChangesAsync(cancellationToken);
        return new ContributorMutationResult(CrudOutcome.Done, contributor.Id);
    }
}
