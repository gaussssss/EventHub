using EventHub.Domain.Entities;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Common.Results;

namespace EventHub.Application.Organizers;

public sealed class CreateOrganizerHandler
    : ICommandHandler<CreateOrganizerCommand, OrganizerMutationResult>
{
    private readonly IOrganizerRepository _organizers;

    public CreateOrganizerHandler(IOrganizerRepository organizers) => _organizers = organizers;

    public async Task<OrganizerMutationResult> HandleAsync(
        CreateOrganizerCommand command, CancellationToken cancellationToken = default)
    {
        var organizer = Organizer.Create(command.Name, command.ContactEmail);
        await _organizers.AddAsync(organizer, cancellationToken);
        await _organizers.SaveChangesAsync(cancellationToken);
        return new OrganizerMutationResult(CrudOutcome.Done, organizer.Id);
    }
}
