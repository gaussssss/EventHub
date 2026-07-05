using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class RegistrationEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public RegistrationEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private async Task<Guid> SeedActivityAsync(int maxParticipants, int alreadyRegistered = 0)
    {
        var activityId = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            // Slug unique par seed : la base in-memory est partagée par la
            // classe de test (index unique sur Slug).
            var category = Category.Create($"sport-{Guid.NewGuid():N}", "Sport");
            var activity = Activity.Create(
                "Yoga matinal", "…", category.Id, null,
                new DateTime(2026, 8, 1, 8, 0, 0, DateTimeKind.Utc), null,
                "Gymnase", "https://img/yoga.jpg", 0, maxParticipants, null, null,
                isFeatured: false, ActivityStatus.Published, DateTime.UtcNow);
            activityId = activity.Id;
            db.Categories.Add(category);
            db.Activities.Add(activity);
            for (var i = 0; i < alreadyRegistered; i++)
            {
                db.Registrations.Add(Registration.Create(
                    Guid.NewGuid(), activity.Id, RegistrationStatus.Registered,
                    Registration.SourceApp, null, DateTime.UtcNow));
            }
            await db.SaveChangesAsync();
        });
        return activityId;
    }

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    [Fact]
    public async Task Register_returns_registered_and_increments_participants()
    {
        var activityId = await SeedActivityAsync(maxParticipants: 10);
        var client = ClientAs(Guid.NewGuid());

        var response = await client.PostAsync($"/api/activities/{activityId}/register", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("registered");

        var detail = await client.GetFromJsonAsync<ActivityResponse>(
            $"/api/activities/{activityId}");
        detail!.CurrentParticipants.Should().Be(1);
    }

    [Fact]
    public async Task Register_twice_is_idempotent()
    {
        var activityId = await SeedActivityAsync(maxParticipants: 10);
        var client = ClientAs(Guid.NewGuid());

        await client.PostAsync($"/api/activities/{activityId}/register", null);
        var second = await client.PostAsync($"/api/activities/{activityId}/register", null);

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        (await second.Content.ReadAsStringAsync()).Should().Contain("alreadyRegistered");
    }

    [Fact]
    public async Task Register_on_full_activity_returns_waitlisted()
    {
        var activityId = await SeedActivityAsync(maxParticipants: 1, alreadyRegistered: 1);
        var client = ClientAs(Guid.NewGuid());

        var response = await client.PostAsync($"/api/activities/{activityId}/register", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("waitlisted");
    }

    [Fact]
    public async Task Register_without_user_header_returns_401()
    {
        var activityId = await SeedActivityAsync(maxParticipants: 10);
        var client = _factory.CreateClient(); // pas d'en-tête X-User-Id

        var response = await client.PostAsync($"/api/activities/{activityId}/register", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Register_on_unknown_activity_returns_404()
    {
        var client = ClientAs(Guid.NewGuid());

        var response = await client.PostAsync(
            $"/api/activities/{Guid.NewGuid()}/register", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cancel_promotes_the_first_waitlisted_and_keeps_count()
    {
        // Activité d'1 place : A inscrit, B en liste d'attente.
        var activityId = await SeedActivityAsync(maxParticipants: 1);
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        await ClientAs(userA).PostAsync($"/api/activities/{activityId}/register", null);
        var bReg = await ClientAs(userB).PostAsync($"/api/activities/{activityId}/register", null);
        (await bReg.Content.ReadAsStringAsync()).Should().Contain("waitlisted");

        // A annule → B doit être promu, la place reste occupée.
        var cancel = await ClientAs(userA).PostAsync($"/api/activities/{activityId}/cancel", null);

        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await cancel.Content.ReadAsStringAsync();
        body.Should().Contain("cancelled");
        body.Should().Contain(userB.ToString());

        var detail = await ClientAs(userA)
            .GetFromJsonAsync<ActivityResponse>($"/api/activities/{activityId}");
        detail!.CurrentParticipants.Should().Be(1);

        // B est désormais bien inscrit (ré-inscription idempotente).
        var bAgain = await ClientAs(userB).PostAsync($"/api/activities/{activityId}/register", null);
        (await bAgain.Content.ReadAsStringAsync()).Should().Contain("alreadyRegistered");
    }

    [Fact]
    public async Task Cancel_when_not_registered_returns_404()
    {
        var activityId = await SeedActivityAsync(maxParticipants: 10);

        var response = await ClientAs(Guid.NewGuid())
            .PostAsync($"/api/activities/{activityId}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record ActivityResponse
    {
        public int CurrentParticipants { get; init; }
    }
}
