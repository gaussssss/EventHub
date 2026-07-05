using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class AdminDashboardTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public AdminDashboardTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Overview_returns_kpis()
    {
        await _factory.WithDbAsync(async db =>
        {
            var u = Guid.NewGuid();
            db.Users.Add(new ApplicationUser { Id = u, UserName = $"u-{u:N}", Name = "K" });
            var category = Category.Create($"cat-{Guid.NewGuid():N}", "Cat");
            var activity = Activity.Create(
                "Dash", "Desc", category.Id, null,
                new DateTime(2027, 1, 1, 10, 0, 0, DateTimeKind.Utc), null,
                "Lieu", "https://img/a.jpg", 10, 30, null, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            db.Categories.Add(category);
            db.Activities.Add(activity);
            db.Registrations.Add(Registration.Create(u, activity.Id, RegistrationStatus.Registered, Registration.SourceApp, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var overview = await _factory.CreateAdminClient()
            .GetFromJsonAsync<OverviewRow>("/api/admin/dashboard/overview");

        overview!.TotalActivities.Should().BeGreaterThan(0);
        overview.PublishedActivities.Should().BeGreaterThan(0);
        overview.TotalRegistrations.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Export_registrations_returns_csv()
    {
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser { Id = userId, UserName = $"u-{userId:N}", Name = "Exp", Email = "exp@uqtr.ca" });
            var category = Category.Create($"cat-{Guid.NewGuid():N}", "Cat");
            var activity = Activity.Create(
                "Export Me", "Desc", category.Id, null,
                new DateTime(2027, 2, 1, 10, 0, 0, DateTimeKind.Utc), null,
                "Lieu", "https://img/a.jpg", 10, 30, null, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            db.Categories.Add(category);
            db.Activities.Add(activity);
            db.Registrations.Add(Registration.Create(userId, activity.Id, RegistrationStatus.Registered, Registration.SourceApp, null, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var response = await _factory.CreateAdminClient()
            .GetAsync("/api/admin/exports/registrations.csv");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");

        var csv = await response.Content.ReadAsStringAsync();
        csv.Should().Contain("activityId,activityTitle,userId,userName,userEmail,status,registeredAt");
        csv.Should().Contain("Export Me");
        csv.Should().Contain("exp@uqtr.ca");
    }

    private sealed record OverviewRow
    {
        public int TotalActivities { get; init; }
        public int PublishedActivities { get; init; }
        public int TotalRegistrations { get; init; }
    }
}
