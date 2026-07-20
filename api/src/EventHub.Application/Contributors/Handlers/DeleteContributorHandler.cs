using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Contributors;

public sealed class DeleteContributorHandler
    : ICommandHandler<DeleteContributorCommand, CrudOutcome>
{
    private readonly IContributorRepository _contributors;

    public DeleteContributorHandler(IContributorRepository contributors) =>
        _contributors = contributors;

    public async Task<CrudOutcome> HandleAsync(
        DeleteContributorCommand command, CancellationToken cancellationToken = default)
    {
        var contributor = await _contributors.GetAsync(command.Id, cancellationToken);
        if (contributor is null)
            return CrudOutcome.NotFound;

        _contributors.Remove(contributor);
        await _contributors.SaveChangesAsync(cancellationToken);
        return CrudOutcome.Done;
    }
}
