using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Organizers;

public sealed class UpdateOrganizerHandler : ICommandHandler<UpdateOrganizerCommand, CrudOutcome>
{
    private readonly IOrganizerRepository _organizers;

    public UpdateOrganizerHandler(IOrganizerRepository organizers) => _organizers = organizers;

    public async Task<CrudOutcome> HandleAsync(
        UpdateOrganizerCommand command, CancellationToken cancellationToken = default)
    {
        var organizer = await _organizers.GetAsync(command.Id, cancellationToken);
        if (organizer is null)
            return CrudOutcome.NotFound;

        organizer.Update(command.Name, command.ContactEmail);
        await _organizers.SaveChangesAsync(cancellationToken);
        return CrudOutcome.Done;
    }
}
