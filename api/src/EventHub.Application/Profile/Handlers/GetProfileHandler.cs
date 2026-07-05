using EventHub.Domain.Repositories;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Hearts;
using EventHub.Application.Registrations;
using EventHub.Domain.ValueObjects;

namespace EventHub.Application.Profile;

/// <summary>Compose le profil : identité + cœurs/niveau + inscriptions actives.</summary>
public sealed class GetProfileHandler : IQueryHandler<GetProfileQuery, ProfileDto?>
{
    private readonly IUserReadRepository _users;
    private readonly IHeartReadRepository _hearts;
    private readonly IRegistrationRepository _registrations;

    public GetProfileHandler(
        IUserReadRepository users,
        IHeartReadRepository hearts,
        IRegistrationRepository registrations)
    {
        _users = users;
        _hearts = hearts;
        _registrations = registrations;
    }

    public async Task<ProfileDto?> HandleAsync(
        GetProfileQuery query, CancellationToken cancellationToken = default)
    {
        var userId = query.UserId;
        var user = await _users.GetAsync(userId, cancellationToken);
        if (user is null)
            return null;

        var total = await _hearts.GetTotalAsync(userId, cancellationToken);
        var level = HeartLevel.FromHearts(total);
        var activityIds = await _registrations.GetActiveActivityIdsAsync(userId, cancellationToken);

        return new ProfileDto
        {
            Id = user.Id,
            Name = user.Name ?? string.Empty,
            Email = user.Email ?? string.Empty,
            AvatarUrl = user.AvatarUrl,
            TotalHearts = total,
            Level = level.Name,
            PreviousLevelThreshold = level.PreviousThreshold,
            NextLevelThreshold = level.NextThreshold,
            RegisteredActivityIds = activityIds,
            RegistrationCount = activityIds.Count,
        };
    }
}
