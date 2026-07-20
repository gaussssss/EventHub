using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class UploadsAndSettingsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public UploadsAndSettingsTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Sign_upload_returns_upload_and_file_urls()
    {
        var ticket = await (await _factory.CreateClient().PostAsJsonAsync(
            "/api/uploads/sign", new { type = "posts", contentType = "image/jpeg" }))
            .Content.ReadFromJsonAsync<TicketRow>();

        ticket!.UploadUrl.Should().NotBeNullOrEmpty();
        ticket.FileUrl.Should().Contain("/posts/");
    }

    [Fact]
    public async Task Upload_avatar_updates_profile()
    {
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser { Id = userId, UserName = $"u-{userId:N}", Name = "Av", Email = "av@uqtr.ca" });
            await db.SaveChangesAsync();
        });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());

        // Le fichier est d'abord uploadé (POST /api/uploads/image) puis son chemin
        // relatif est enregistré sur le profil (POST /api/me/avatar) — on persiste
        // le chemin tel quel, sans nom de domaine.
        const string avatarPath = "/uploads/abc123.png";
        var response = await client.PostAsJsonAsync(
            "/api/me/avatar", new { avatarUrl = avatarPath });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<AvatarRow>();
        payload!.AvatarUrl.Should().Be(avatarPath);
    }

    [Fact]
    public async Task Gamification_settings_get_then_patch()
    {
        var admin = _factory.CreateAdminClient();

        var defaults = await admin.GetFromJsonAsync<SettingsRow>("/api/admin/settings/gamification");
        defaults!.SilverThreshold.Should().Be(200);
        defaults.GoldThreshold.Should().Be(500);

        var updated = await (await admin.PatchAsJsonAsync("/api/admin/settings/gamification",
            new { silverThreshold = 300, goldThreshold = 800, defaultAttendanceReward = 25 }))
            .Content.ReadFromJsonAsync<SettingsRow>();
        updated!.SilverThreshold.Should().Be(300);
        updated.GoldThreshold.Should().Be(800);

        (await admin.GetFromJsonAsync<SettingsRow>("/api/admin/settings/gamification"))!
            .SilverThreshold.Should().Be(300);
    }

    [Fact]
    public async Task Gamification_patch_rejects_gold_below_silver()
    {
        var response = await _factory.CreateAdminClient().PatchAsJsonAsync(
            "/api/admin/settings/gamification",
            new { silverThreshold = 500, goldThreshold = 300, defaultAttendanceReward = 10 });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record TicketRow { public string UploadUrl { get; init; } = ""; public string FileUrl { get; init; } = ""; }
    private sealed record AvatarRow { public string AvatarUrl { get; init; } = ""; }
    private sealed record SettingsRow { public int SilverThreshold { get; init; } public int GoldThreshold { get; init; } }
}
