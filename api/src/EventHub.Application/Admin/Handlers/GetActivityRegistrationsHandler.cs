using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Admin;

public sealed class GetActivityRegistrationsHandler
    : IQueryHandler<GetActivityRegistrationsQuery, IReadOnlyList<RegistrationEntryDto>>
{
    private readonly IRegistrationReadRepository _registrations;

    public GetActivityRegistrationsHandler(IRegistrationReadRepository registrations) =>
        _registrations = registrations;

    public Task<IReadOnlyList<RegistrationEntryDto>> HandleAsync(
        GetActivityRegistrationsQuery query, CancellationToken cancellationToken = default) =>
        _registrations.GetByActivityAsync(query.ActivityId, cancellationToken);
}
