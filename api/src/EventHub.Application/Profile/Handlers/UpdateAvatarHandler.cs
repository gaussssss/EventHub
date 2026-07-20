using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

public sealed class UpdateAvatarHandler : ICommandHandler<UpdateAvatarCommand, AvatarResult>
{
    private readonly IUserProfileService _profiles;

    public UpdateAvatarHandler(IUserProfileService profiles)
    {
        _profiles = profiles;
    }

    public async Task<AvatarResult> HandleAsync(
        UpdateAvatarCommand command, CancellationToken cancellationToken = default)
    {
        // Le fichier est déjà stocké (POST /api/uploads/image) ; on persiste le
        // chemin fourni tel quel (relatif, résolu côté client contre sa base API).
        var updated = await _profiles.UpdateAvatarAsync(
            command.UserId, command.AvatarUrl, cancellationToken);

        return updated
            ? new AvatarResult(true, command.AvatarUrl)
            : new AvatarResult(false, null);
    }
}
