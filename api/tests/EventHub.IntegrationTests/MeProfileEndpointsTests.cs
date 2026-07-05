using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class MeProfileEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public MeProfileEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    [Fact]
    public async Task PATCH_me_updates_display_name()
    {
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"u-{userId:N}",
                Email = "jo@uqtr.ca",
                Name = "Ancien Nom",
            });
            await db.SaveChangesAsync();
        });

        var response = await ClientAs(userId).PatchAsJsonAsync(
            "/api/me", new { name = "Nouveau Nom" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<ProfileRow>();
        profile!.Name.Should().Be("Nouveau Nom");
    }

    [Fact]
    public async Task GET_me_registrations_lists_registered_activities()
    {
        var userId = Guid.NewGuid();
        var registeredId = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId, UserName = $"u-{userId:N}", Email = "mel@uqtr.ca", Name = "Mel",
            });
            var category = Category.Create($"cat-{Guid.NewGuid():N}", "Cat");
            db.Categories.Add(category);

            var joined = Activity.Create(
                "Atelier céramique", "Initiation", category.Id, null,
                new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc), null,
                "Atelier", "https://img/c.jpg", 20, 15, null, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            var other = Activity.Create(
                "Course", "5km", category.Id, null,
                new DateTime(2026, 10, 2, 18, 0, 0, DateTimeKind.Utc), null,
                "Parc", "https://img/r.jpg", 10, 100, null, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            registeredId = joined.Id;
            db.Activities.AddRange(joined, other);
            db.Registrations.Add(Registration.Create(
                userId, joined.Id, RegistrationStatus.Registered,
                Registration.SourceApp, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var mine = await ClientAs(userId)
            .GetFromJsonAsync<List<ActivityRow>>("/api/me/registrations");

        mine!.Should().ContainSingle().Which.Id.Should().Be(registeredId);
    }

    private sealed record ProfileRow
    {
        public string Name { get; init; } = "";
    }

    private sealed record ActivityRow
    {
        public Guid Id { get; init; }
    }
}
