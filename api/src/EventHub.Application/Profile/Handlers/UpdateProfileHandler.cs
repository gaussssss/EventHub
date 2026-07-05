using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

/// <summary>Renomme l'utilisateur courant. Renvoie <c>false</c> s'il est introuvable.</summary>
public sealed class UpdateProfileHandler : ICommandHandler<UpdateProfileCommand, bool>
{
    private readonly IUserProfileService _profiles;

    public UpdateProfileHandler(IUserProfileService profiles) => _profiles = profiles;

    public Task<bool> HandleAsync(
        UpdateProfileCommand command, CancellationToken cancellationToken = default)
    {
        var name = command.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Le nom ne peut pas être vide.", nameof(command));

        return _profiles.UpdateNameAsync(command.UserId, name, cancellationToken);
    }
}
