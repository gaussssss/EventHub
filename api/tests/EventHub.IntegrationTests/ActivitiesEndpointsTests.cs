using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class ActivitiesEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public ActivitiesEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_activities_returns_published_activities_with_participant_count()
    {
        // Arrange : une catégorie, une activité publiée, une inscription active.
        var activityId = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create("sport", "Sport");
            var activity = Activity.Create(
                "Tournoi de volleyball", "Championnat du campus", category.Id, null,
                new DateTime(2026, 7, 14, 10, 0, 0, DateTimeKind.Utc), null,
                "Gymnase UQTR", "https://img/volley.jpg", 40, 48, null, null,
                isFeatured: false, ActivityStatus.Published, DateTime.UtcNow);
            activityId = activity.Id;
            db.Categories.Add(category);
            db.Activities.Add(activity);
            db.Registrations.Add(Registration.Create(
                Guid.NewGuid(), activity.Id, RegistrationStatus.Registered,
                Registration.SourceApp, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/activities");

        // Assert : 200 + payload camelCase + comptage d'inscrits.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var raw = await response.Content.ReadAsStringAsync();
        raw.Should().Contain("\"heartsReward\"");   // camelCase respecté
        raw.Should().Contain("\"currentParticipants\"");

        var activities = await response.Content
            .ReadFromJsonAsync<List<ActivityResponse>>();
        activities.Should().NotBeNull();
        var volley = activities!.Single(a => a.Id == activityId);
        volley.Title.Should().Be("Tournoi de volleyball");
        volley.Category.Should().Be("sport");
        volley.CurrentParticipants.Should().Be(1);
        volley.HeartsReward.Should().Be(40);
    }

    [Fact]
    public async Task GET_activity_by_unknown_id_returns_404()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/activities/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record ActivityResponse
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
        public string Category { get; init; } = "";
        public int CurrentParticipants { get; init; }
        public int HeartsReward { get; init; }
    }
}
