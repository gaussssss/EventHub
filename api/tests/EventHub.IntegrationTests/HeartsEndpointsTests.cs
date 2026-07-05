using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class HeartsEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public HeartsEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private async Task<Guid> SeedActivityAsync(int heartsReward)
    {
        var activityId = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create($"sport-{Guid.NewGuid():N}", "Sport");
            var activity = Activity.Create(
                "Tournoi de basketball", "…", category.Id, null,
                new DateTime(2026, 7, 11, 14, 0, 0, DateTimeKind.Utc), null,
                "Complexe sportif", "https://img/basket.jpg", heartsReward, 60, null, null,
                isFeatured: false, ActivityStatus.Published, DateTime.UtcNow);
            activityId = activity.Id;
            db.Categories.Add(category);
            db.Activities.Add(activity);
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
    public async Task Attendance_credits_hearts_and_me_hearts_reflects_it()
    {
        var activityId = await SeedActivityAsync(heartsReward: 40);
        var userId = Guid.NewGuid();

        // L'utilisateur s'inscrit, puis le back office marque sa présence.
        await ClientAs(userId).PostAsync($"/api/activities/{activityId}/register", null);

        var attendance = await _factory.CreateAdminClient().PostAsJsonAsync(
            $"/api/admin/activities/{activityId}/attendance",
            new { userIds = new[] { userId } });

        attendance.StatusCode.Should().Be(HttpStatusCode.OK);
        (await attendance.Content.ReadAsStringAsync()).Should().Contain("\"credited\":1");

        // GET /me/hearts reflète le crédit + le niveau calculé.
        var hearts = await ClientAs(userId)
            .GetFromJsonAsync<HeartsResponse>("/api/me/hearts");

        hearts!.TotalHearts.Should().Be(40);
        hearts.Level.Should().Be("Bronze");     // 40 < 200
        hearts.NextThreshold.Should().Be(200);
        hearts.History.Should().ContainSingle(h => h.ActivityTitle == "Tournoi de basketball");
    }

    [Fact]
    public async Task Marking_attendance_twice_does_not_double_credit()
    {
        var activityId = await SeedActivityAsync(heartsReward: 25);
        var userId = Guid.NewGuid();
        await ClientAs(userId).PostAsync($"/api/activities/{activityId}/register", null);

        var admin = _factory.CreateAdminClient();
        var body = new { userIds = new[] { userId } };
        await admin.PostAsJsonAsync($"/api/admin/activities/{activityId}/attendance", body);
        var second = await admin.PostAsJsonAsync(
            $"/api/admin/activities/{activityId}/attendance", body);

        (await second.Content.ReadAsStringAsync()).Should().Contain("\"credited\":0");

        var hearts = await ClientAs(userId).GetFromJsonAsync<HeartsResponse>("/api/me/hearts");
        hearts!.TotalHearts.Should().Be(25);
    }

    [Fact]
    public async Task Me_hearts_without_user_returns_401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/me/hearts");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record HeartsResponse
    {
        public int TotalHearts { get; init; }
        public string Level { get; init; } = "";
        public int NextThreshold { get; init; }
        public List<HistoryItem> History { get; init; } = new();

        public sealed record HistoryItem
        {
            public string ActivityTitle { get; init; } = "";
            public int Hearts { get; init; }
        }
    }
}
