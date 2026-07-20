using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Hearts;
using EventHub.Application.Notifications;
using EventHub.Application.Profile;
using EventHub.Application.Registrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/me")]
public class MeController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUser _currentUser;

    public MeController(ISender sender, ICurrentUser currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    /// <summary>Profil de l'utilisateur courant (GET /api/me).</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var profile = await _sender.Send(new GetProfileQuery(userId.Value), cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    /// <summary>Résumé « cœurs santé » de l'utilisateur courant (GET /api/me/hearts).</summary>
    [HttpGet("hearts")]
    public async Task<IActionResult> Hearts(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        return Ok(await _sender.Send(new GetHeartsSummaryQuery(userId.Value), cancellationToken));
    }

    /// <summary>Activités inscrites de l'utilisateur (GET /api/me/registrations).</summary>
    [HttpGet("registrations")]
    public async Task<IActionResult> Registrations(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        return Ok(await _sender.Send(new GetMyRegistrationsQuery(userId.Value), cancellationToken));
    }

    public sealed record UpdateProfileBody(string Name);

    /// <summary>Modifier son profil (PATCH /api/me) — nom d'affichage.</summary>
    [HttpPatch]
    public async Task<IActionResult> Update(
        [FromBody] UpdateProfileBody body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var updated = await _sender.Send(
            new UpdateProfileCommand(userId.Value, body.Name), cancellationToken);
        if (!updated)
            return NotFound();

        var profile = await _sender.Send(new GetProfileQuery(userId.Value), cancellationToken);
        return Ok(profile);
    }

    public sealed record UpdateAvatarBody(string AvatarUrl);

    /// <summary>
    /// Enregistrer la photo de profil (POST /api/me/avatar). Le fichier a d'abord
    /// été envoyé via POST /api/uploads/image, qui renvoie un chemin ; on persiste
    /// ce chemin ici. On accepte un chemin relatif (<c>/uploads/…</c>) ou une URL
    /// http(s) absolue (avatar externe).
    /// </summary>
    [HttpPost("avatar")]
    public async Task<IActionResult> UpdateAvatar(
        [FromBody] UpdateAvatarBody body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var avatarUrl = body.AvatarUrl?.Trim();
        if (string.IsNullOrEmpty(avatarUrl))
            return BadRequest(new { error = "avatarUrl requis" });
        if (!avatarUrl.StartsWith("/uploads/", StringComparison.Ordinal) &&
            !avatarUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !avatarUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "avatarUrl invalide" });

        var result = await _sender.Send(
            new UpdateAvatarCommand(userId.Value, avatarUrl), cancellationToken);
        return result.Updated
            ? Ok(new { avatarUrl = result.AvatarUrl })
            : NotFound();
    }

    public sealed record RegisterDeviceBody(string PushToken, string? Platform);

    /// <summary>Enregistrer un jeton push (POST /api/me/devices).</summary>
    [HttpPost("devices")]
    public async Task<IActionResult> RegisterDevice(
        [FromBody] RegisterDeviceBody body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();
        if (string.IsNullOrWhiteSpace(body.PushToken))
            return BadRequest(new { error = "pushToken requis" });

        var deviceId = await _sender.Send(
            new RegisterDeviceCommand(userId.Value, body.PushToken, body.Platform),
            cancellationToken);
        return Ok(new { deviceId });
    }

    /// <summary>Fil des notifications (GET /api/me/notifications).</summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> Notifications(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        return Ok(await _sender.Send(new GetMyNotificationsQuery(userId.Value), cancellationToken));
    }

    /// <summary>Marquer une notification comme lue (PATCH /api/me/notifications/{id}/read).</summary>
    [HttpPatch("notifications/{id:guid}/read")]
    public async Task<IActionResult> MarkNotificationRead(
        Guid id, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        var status = await _sender.Send(
            new MarkNotificationReadCommand(userId.Value, id), cancellationToken);
        return status switch
        {
            MarkReadStatus.Marked => NoContent(),
            MarkReadStatus.NotFound => NotFound(),
            MarkReadStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden),
            _ => StatusCode(StatusCodes.Status500InternalServerError),
        };
    }

    /// <summary>Préférences de notification (GET /api/me/notification-settings).</summary>
    [HttpGet("notification-settings")]
    public async Task<IActionResult> NotificationSettings(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        return Ok(await _sender.Send(
            new GetNotificationSettingsQuery(userId.Value), cancellationToken));
    }

    public sealed record NotificationSettingsBody(
        bool EventReminders, bool WaitlistPromotions, bool HeartsEarned, bool NewComments);

    /// <summary>Modifier ses préférences (PATCH /api/me/notification-settings).</summary>
    [HttpPatch("notification-settings")]
    public async Task<IActionResult> UpdateNotificationSettings(
        [FromBody] NotificationSettingsBody body, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized();

        return Ok(await _sender.Send(
            new UpdateNotificationSettingsCommand(
                userId.Value, body.EventReminders, body.WaitlistPromotions,
                body.HeartsEarned, body.NewComments),
            cancellationToken));
    }
}
