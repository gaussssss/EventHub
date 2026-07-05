using System.Net;
using System.Net.Http.Json;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class NotificationsEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public NotificationsEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private async Task SeedUserAsync(Guid userId)
    {
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser { Id = userId, UserName = $"u-{userId:N}", Name = "N" });
            await db.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task Register_device_is_idempotent_per_token()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var client = ClientAs(userId);
        var token = $"tok-{Guid.NewGuid():N}";

        var first = await (await client.PostAsJsonAsync("/api/me/devices",
            new { pushToken = token, platform = "ios" })).Content.ReadFromJsonAsync<DeviceRow>();
        var second = await (await client.PostAsJsonAsync("/api/me/devices",
            new { pushToken = token, platform = "ios" })).Content.ReadFromJsonAsync<DeviceRow>();

        second!.DeviceId.Should().Be(first!.DeviceId);
    }

    [Fact]
    public async Task Broadcast_delivers_to_users_and_can_be_marked_read()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);

        var recipients = await (await _factory.CreateAdminClient().PostAsJsonAsync(
            "/api/admin/notifications/broadcast",
            new { audience = "all", title = "Bienvenue", body = "Nouveau semestre !" }))
            .Content.ReadFromJsonAsync<BroadcastRow>();
        recipients!.Recipients.Should().BeGreaterThan(0);

        var client = ClientAs(userId);
        var notifications = await client.GetFromJsonAsync<List<NotifRow>>("/api/me/notifications");
        var mine = notifications!.Single(n => n.Title == "Bienvenue");
        mine.ReadAt.Should().BeNull();

        (await client.PatchAsync($"/api/me/notifications/{mine.Id}/read", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var after = await client.GetFromJsonAsync<List<NotifRow>>("/api/me/notifications");
        after!.Single(n => n.Id == mine.Id).ReadAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Notification_settings_default_true_then_updatable()
    {
        var userId = Guid.NewGuid();
        await SeedUserAsync(userId);
        var client = ClientAs(userId);

        var defaults = await client.GetFromJsonAsync<SettingsRow>("/api/me/notification-settings");
        defaults!.EventReminders.Should().BeTrue();

        var updated = await (await client.PatchAsJsonAsync("/api/me/notification-settings",
            new { eventReminders = false, waitlistPromotions = true, heartsEarned = true, newComments = false }))
            .Content.ReadFromJsonAsync<SettingsRow>();
        updated!.EventReminders.Should().BeFalse();
        updated.NewComments.Should().BeFalse();

        var reread = await client.GetFromJsonAsync<SettingsRow>("/api/me/notification-settings");
        reread!.EventReminders.Should().BeFalse();
    }

    private sealed record DeviceRow { public Guid DeviceId { get; init; } }
    private sealed record BroadcastRow { public int Recipients { get; init; } }
    private sealed record NotifRow { public Guid Id { get; init; } public string Title { get; init; } = ""; public DateTime? ReadAt { get; init; } }
    private sealed record SettingsRow { public bool EventReminders { get; init; } public bool NewComments { get; init; } }
}
