using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Profile;

public sealed class UpdateAvatarHandler : ICommandHandler<UpdateAvatarCommand, AvatarResult>
{
    private readonly IStorageService _storage;
    private readonly IUserProfileService _profiles;

    public UpdateAvatarHandler(IStorageService storage, IUserProfileService profiles)
    {
        _storage = storage;
        _profiles = profiles;
    }

    public async Task<AvatarResult> HandleAsync(
        UpdateAvatarCommand command, CancellationToken cancellationToken = default)
    {
        var ticket = _storage.CreateUploadTicket("avatars", command.ContentType);
        var updated = await _profiles.UpdateAvatarAsync(
            command.UserId, ticket.FileUrl, cancellationToken);

        return updated ? new AvatarResult(true, ticket.FileUrl) : new AvatarResult(false, null);
    }
}
