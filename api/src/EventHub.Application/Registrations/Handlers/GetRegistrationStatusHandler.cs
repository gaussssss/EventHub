using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Registrations;

public sealed class GetRegistrationStatusHandler
    : IQueryHandler<GetRegistrationStatusQuery, RegistrationStatusDto>
{
    private readonly IRegistrationRepository _registrations;

    public GetRegistrationStatusHandler(IRegistrationRepository registrations) =>
        _registrations = registrations;

    public async Task<RegistrationStatusDto> HandleAsync(
        GetRegistrationStatusQuery query, CancellationToken cancellationToken = default)
    {
        var registration = await _registrations.FindAsync(
            query.UserId, query.ActivityId, cancellationToken);

        if (registration is null || registration.Status == RegistrationStatus.Cancelled)
            return new RegistrationStatusDto(false, null, null);

        return new RegistrationStatusDto(
            IsRegistered: true,
            Status: registration.Status.ToString().ToLowerInvariant(),
            RegisteredAt: registration.RegisteredAt);
    }
}
