using System.Net;
using System.Net.Http.Json;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class AdminUsersEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public AdminUsersEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private async Task<Guid> SeedUserAsync(string name, string email)
    {
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                Email = email,
                Name = name,
            });
            await db.SaveChangesAsync();
        });
        return userId;
    }

    [Fact]
    public async Task Search_filters_by_name_and_defaults_role_to_student()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var userId = await SeedUserAsync($"Zoé {marker}", $"{marker}@uqtr.ca");

        var results = await _factory.CreateAdminClient()
            .GetFromJsonAsync<List<AdminUserResponse>>($"/api/admin/users?q={marker}");

        var user = results!.Single(u => u.Id == userId);
        user.Role.Should().Be("student");
        user.Status.Should().Be("active");
        user.TotalHearts.Should().Be(0);
    }

    [Fact]
    public async Task Patch_assigns_role_reflected_in_search()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var userId = await SeedUserAsync($"Léa {marker}", $"{marker}@uqtr.ca");
        var admin = _factory.CreateAdminClient();

        var patch = await admin.PatchAsJsonAsync(
            $"/api/admin/users/{userId}", new { role = "organizer" });
        patch.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var results = await admin.GetFromJsonAsync<List<AdminUserResponse>>($"/api/admin/users?q={marker}");
        results!.Single(u => u.Id == userId).Role.Should().Be("organizer");
    }

    [Fact]
    public async Task Patch_can_suspend_user()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var userId = await SeedUserAsync($"Max {marker}", $"{marker}@uqtr.ca");
        var admin = _factory.CreateAdminClient();

        await admin.PatchAsJsonAsync($"/api/admin/users/{userId}", new { status = "suspended" });

        var results = await admin.GetFromJsonAsync<List<AdminUserResponse>>($"/api/admin/users?q={marker}");
        results!.Single(u => u.Id == userId).Status.Should().Be("suspended");
    }

    [Fact]
    public async Task Patch_invalid_role_returns_400()
    {
        var userId = await SeedUserAsync("Bob", $"{Guid.NewGuid():N}@uqtr.ca");
        var response = await _factory.CreateAdminClient()
            .PatchAsJsonAsync($"/api/admin/users/{userId}", new { role = "wizard" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Patch_unknown_user_returns_404()
    {
        var response = await _factory.CreateAdminClient()
            .PatchAsJsonAsync($"/api/admin/users/{Guid.NewGuid()}", new { role = "admin" });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Adjust_hearts_updates_total_shown_in_search()
    {
        var marker = Guid.NewGuid().ToString("N")[..8];
        var userId = await SeedUserAsync($"Ana {marker}", $"{marker}@uqtr.ca");
        var admin = _factory.CreateAdminClient();

        var adjust = await admin.PostAsJsonAsync(
            $"/api/admin/users/{userId}/hearts", new { hearts = 150, reason = "bonus" });
        adjust.StatusCode.Should().Be(HttpStatusCode.OK);
        (await adjust.Content.ReadAsStringAsync()).Should().Contain("\"totalHearts\":150");

        var results = await admin.GetFromJsonAsync<List<AdminUserResponse>>($"/api/admin/users?q={marker}");
        results!.Single(u => u.Id == userId).TotalHearts.Should().Be(150);
    }

    [Fact]
    public async Task Adjust_hearts_zero_returns_400()
    {
        var userId = await SeedUserAsync("Nil", $"{Guid.NewGuid():N}@uqtr.ca");
        var response = await _factory.CreateAdminClient()
            .PostAsJsonAsync($"/api/admin/users/{userId}/hearts", new { hearts = 0, reason = "x" });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record AdminUserResponse
    {
        public Guid Id { get; init; }
        public string Role { get; init; } = "";
        public string Status { get; init; } = "";
        public int TotalHearts { get; init; }
    }
}
