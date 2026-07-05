using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class AdminActivityActionsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public AdminActivityActionsTests(EventHubApiFactory factory) => _factory = factory;

    private async Task<Guid> SeedActivityAsync(
        ActivityStatus status = ActivityStatus.Published, bool featured = false)
    {
        var id = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create($"cat-{Guid.NewGuid():N}", "Cat");
            var activity = Activity.Create(
                "Activité", "Desc", category.Id, null,
                new DateTime(2026, 12, 1, 10, 0, 0, DateTimeKind.Utc), null,
                "Lieu", "https://img/a.jpg", 10, 30, null, null,
                featured, status, DateTime.UtcNow);
            id = activity.Id;
            db.Categories.Add(category);
            db.Activities.Add(activity);
            await db.SaveChangesAsync();
        });
        return id;
    }

    [Fact]
    public async Task Cancel_then_publish_toggles_catalogue_presence()
    {
        var id = await SeedActivityAsync();
        var admin = _factory.CreateAdminClient();

        (await admin.PostAsync($"/api/admin/activities/{id}/cancel", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await admin.GetFromJsonAsync<List<Item>>("/api/activities"))!
            .Should().NotContain(a => a.Id == id);

        (await admin.PostAsync($"/api/admin/activities/{id}/publish", null))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await admin.GetFromJsonAsync<List<Item>>("/api/activities"))!
            .Should().Contain(a => a.Id == id);
    }

    [Fact]
    public async Task Feature_toggles_featured_flag()
    {
        var id = await SeedActivityAsync(featured: false);
        var admin = _factory.CreateAdminClient();

        var response = await admin.PostAsync($"/api/admin/activities/{id}/feature", null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<FeatureRow>();
        payload!.IsFeatured.Should().BeTrue();

        (await admin.GetFromJsonAsync<List<Item>>("/api/activities/featured"))!
            .Should().Contain(a => a.Id == id);
    }

    [Fact]
    public async Task Publish_unknown_returns_404()
    {
        var response = await _factory.CreateAdminClient()
            .PostAsync($"/api/admin/activities/{Guid.NewGuid()}/publish", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Registrations_lists_participants_and_waitlist()
    {
        var id = await SeedActivityAsync();
        var registered = Guid.NewGuid();
        var waitlisted = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser { Id = registered, UserName = $"u-{registered:N}", Name = "Reg", Email = "reg@uqtr.ca" });
            db.Users.Add(new ApplicationUser { Id = waitlisted, UserName = $"u-{waitlisted:N}", Name = "Wait", Email = "wait@uqtr.ca" });
            db.Registrations.Add(Registration.Create(registered, id, RegistrationStatus.Registered, Registration.SourceApp, null, DateTime.UtcNow));
            db.Registrations.Add(Registration.Create(waitlisted, id, RegistrationStatus.Waitlisted, Registration.SourceApp, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var list = await _factory.CreateAdminClient()
            .GetFromJsonAsync<List<RegRow>>($"/api/admin/activities/{id}/registrations");

        list!.Should().HaveCount(2);
        list.Should().Contain(r => r.Name == "Reg" && r.Status == "registered");
        list.Should().Contain(r => r.Name == "Wait" && r.Status == "waitlisted");
    }

    private sealed record Item
    {
        public Guid Id { get; init; }
    }

    private sealed record FeatureRow
    {
        public bool IsFeatured { get; init; }
    }

    private sealed record RegRow
    {
        public string? Name { get; init; }
        public string Status { get; init; } = "";
    }
}
