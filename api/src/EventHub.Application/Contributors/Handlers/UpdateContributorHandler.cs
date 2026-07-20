using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Contributors;

public sealed class UpdateContributorHandler
    : ICommandHandler<UpdateContributorCommand, CrudOutcome>
{
    private readonly IContributorRepository _contributors;

    public UpdateContributorHandler(IContributorRepository contributors) =>
        _contributors = contributors;

    public async Task<CrudOutcome> HandleAsync(
        UpdateContributorCommand command, CancellationToken cancellationToken = default)
    {
        var contributor = await _contributors.GetAsync(command.Id, cancellationToken);
        if (contributor is null)
            return CrudOutcome.NotFound;

        contributor.Update(
            command.Name, command.Role, command.AvatarUrl, command.SortOrder);
        await _contributors.SaveChangesAsync(cancellationToken);
        return CrudOutcome.Done;
    }
}
