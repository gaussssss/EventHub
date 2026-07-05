using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Organizers;

public sealed class DeleteOrganizerHandler : ICommandHandler<DeleteOrganizerCommand, CrudOutcome>
{
    private readonly IOrganizerRepository _organizers;

    public DeleteOrganizerHandler(IOrganizerRepository organizers) => _organizers = organizers;

    public async Task<CrudOutcome> HandleAsync(
        DeleteOrganizerCommand command, CancellationToken cancellationToken = default)
    {
        var organizer = await _organizers.GetAsync(command.Id, cancellationToken);
        if (organizer is null)
            return CrudOutcome.NotFound;

        _organizers.Remove(organizer);
        await _organizers.SaveChangesAsync(cancellationToken);
        return CrudOutcome.Done;
    }
}
