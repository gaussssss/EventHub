using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class RegistrationStatusTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public RegistrationStatusTests(EventHubApiFactory factory) => _factory = factory;

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private async Task<Guid> SeedActivityAsync(string? registrationUrl = null)
    {
        var id = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create($"cat-{Guid.NewGuid():N}", "Cat");
            var activity = Activity.Create(
                "Yoga", "Détente", category.Id, null,
                new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc), null,
                "Salle", "https://img/y.jpg", 15, 20, registrationUrl, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            id = activity.Id;
            db.Categories.Add(category);
            db.Activities.Add(activity);
            await db.SaveChangesAsync();
        });
        return id;
    }

    [Fact]
    public async Task GET_registration_reflects_registered_state()
    {
        var activityId = await SeedActivityAsync();
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Registrations.Add(Registration.Create(
                userId, activityId, RegistrationStatus.Registered,
                Registration.SourceApp, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var status = await ClientAs(userId)
            .GetFromJsonAsync<StatusRow>($"/api/activities/{activityId}/registration");

        status!.IsRegistered.Should().BeTrue();
        status.Status.Should().Be("registered");
    }

    [Fact]
    public async Task GET_registration_is_false_when_not_registered()
    {
        var activityId = await SeedActivityAsync();
        var status = await ClientAs(Guid.NewGuid())
            .GetFromJsonAsync<StatusRow>($"/api/activities/{activityId}/registration");

        status!.IsRegistered.Should().BeFalse();
    }

    [Fact]
    public async Task GET_registration_url_returns_form_link()
    {
        var activityId = await SeedActivityAsync("https://forms.google.com/abc");
        var payload = await _factory.CreateClient()
            .GetFromJsonAsync<UrlRow>($"/api/activities/{activityId}/registration-url");
        payload!.Url.Should().Be("https://forms.google.com/abc");
    }

    [Fact]
    public async Task GET_share_returns_deep_link_and_title()
    {
        var activityId = await SeedActivityAsync();
        var payload = await _factory.CreateClient()
            .GetFromJsonAsync<ShareRow>($"/api/activities/{activityId}/share");
        payload!.ShareUrl.Should().Contain(activityId.ToString());
        payload.Title.Should().Be("Yoga");
    }

    [Fact]
    public async Task GET_share_unknown_activity_returns_404()
    {
        var response = await _factory.CreateClient()
            .GetAsync($"/api/activities/{Guid.NewGuid()}/share");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record StatusRow
    {
        public bool IsRegistered { get; init; }
        public string? Status { get; init; }
    }

    private sealed record UrlRow
    {
        public string? Url { get; init; }
    }

    private sealed record ShareRow
    {
        public string ShareUrl { get; init; } = "";
        public string Title { get; init; } = "";
    }
}
