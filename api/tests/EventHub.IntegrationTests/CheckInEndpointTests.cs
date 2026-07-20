using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventHub.IntegrationTests;

/// <summary>
/// Auto-émargement par QR (POST /api/activities/{id}/check-in) : jeton secret,
/// fenêtre horaire, inscription requise, idempotence du crédit de cœurs.
/// </summary>
public class CheckInEndpointTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public CheckInEndpointTests(EventHubApiFactory factory) => _factory = factory;

    /// <summary>Seed une activité (+ inscription éventuelle) et renvoie (id, jeton).</summary>
    private async Task<(Guid ActivityId, Guid Token)> SeedAsync(
        DateTime startsAtUtc, int heartsReward, Guid? registeredUserId)
    {
        var activityId = Guid.Empty;
        var token = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create($"sport-{Guid.NewGuid():N}", "Sport");
            var activity = Activity.Create(
                "Course à pied", "…", category.Id, null,
                startsAtUtc, startsAtUtc.AddHours(2),
                "Stade", "https://img/course.jpg", heartsReward, 30, null, null,
                isFeatured: false, ActivityStatus.Published, DateTime.UtcNow);
            activityId = activity.Id;
            token = activity.CheckInToken;
            db.Categories.Add(category);
            db.Activities.Add(activity);
            if (registeredUserId is not null)
            {
                db.Registrations.Add(Registration.Create(
                    registeredUserId.Value, activity.Id, RegistrationStatus.Registered,
                    Registration.SourceApp, null, DateTime.UtcNow));
            }
            await db.SaveChangesAsync();
        });
        return (activityId, token);
    }

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private static HttpContent Body(Guid token) =>
        JsonContent.Create(new { token });

    [Fact]
    public async Task CheckIn_with_valid_token_marks_attended_and_awards_hearts()
    {
        var userId = Guid.NewGuid();
        var (activityId, token) =
            await SeedAsync(DateTime.UtcNow, heartsReward: 25, registeredUserId: userId);

        var response = await ClientAs(userId)
            .PostAsync($"/api/activities/{activityId}/check-in", Body(token));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<CheckInRow>();
        payload!.Status.Should().Be("ok");
        payload.HeartsAwarded.Should().Be(25);

        await _factory.WithDbAsync(async db =>
        {
            var reg = await db.Registrations.SingleAsync(
                r => r.ActivityId == activityId && r.UserId == userId);
            reg.Status.Should().Be(RegistrationStatus.Attended);
        });
    }

    [Fact]
    public async Task CheckIn_twice_does_not_double_credit()
    {
        var userId = Guid.NewGuid();
        var (activityId, token) =
            await SeedAsync(DateTime.UtcNow, heartsReward: 25, registeredUserId: userId);
        var client = ClientAs(userId);

        await client.PostAsync($"/api/activities/{activityId}/check-in", Body(token));
        var second = await client
            .PostAsync($"/api/activities/{activityId}/check-in", Body(token));

        var payload = await second.Content.ReadFromJsonAsync<CheckInRow>();
        payload!.Status.Should().Be("ok");
        payload.AlreadyCheckedIn.Should().BeTrue();
        payload.HeartsAwarded.Should().Be(0);
    }

    [Fact]
    public async Task CheckIn_with_wrong_token_is_rejected()
    {
        var userId = Guid.NewGuid();
        var (activityId, _) =
            await SeedAsync(DateTime.UtcNow, heartsReward: 25, registeredUserId: userId);

        var response = await ClientAs(userId)
            .PostAsync($"/api/activities/{activityId}/check-in", Body(Guid.NewGuid()));

        (await response.Content.ReadFromJsonAsync<CheckInRow>())!
            .Status.Should().Be("invalidToken");
    }

    [Fact]
    public async Task CheckIn_when_not_registered_is_rejected()
    {
        var (activityId, token) =
            await SeedAsync(DateTime.UtcNow, heartsReward: 25, registeredUserId: null);

        var response = await ClientAs(Guid.NewGuid())
            .PostAsync($"/api/activities/{activityId}/check-in", Body(token));

        (await response.Content.ReadFromJsonAsync<CheckInRow>())!
            .Status.Should().Be("notRegistered");
    }

    [Fact]
    public async Task CheckIn_far_from_event_window_is_rejected()
    {
        var userId = Guid.NewGuid();
        var (activityId, token) = await SeedAsync(
            DateTime.UtcNow.AddDays(10), heartsReward: 25, registeredUserId: userId);

        var response = await ClientAs(userId)
            .PostAsync($"/api/activities/{activityId}/check-in", Body(token));

        (await response.Content.ReadFromJsonAsync<CheckInRow>())!
            .Status.Should().Be("outsideWindow");
    }

    private sealed record CheckInRow
    {
        public string Status { get; init; } = "";
        public int HeartsAwarded { get; init; }
        public bool AlreadyCheckedIn { get; init; }
    }
}
