using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class ProfileAndAdminEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public ProfileAndAdminEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private async Task<Guid> SeedCategoryAsync()
    {
        var id = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create($"sport-{Guid.NewGuid():N}", "Sport");
            id = category.Id;
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        });
        return id;
    }

    private object ValidActivityBody(Guid categoryId, string title, string? status = "published") => new
    {
        title,
        description = "Description",
        categoryId,
        startsAt = new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc),
        location = "Campus UQTR",
        imageUrl = "https://img/a.jpg",
        heartsReward = 30,
        maxParticipants = 40,
        isFeatured = false,
        status,
    };

    [Fact]
    public async Task Get_me_returns_profile_with_hearts_and_level()
    {
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"u-{userId:N}",
                Email = "alex@uqtr.ca",
                Name = "Alex Tremblay",
            });
            db.HeartTransactions.Add(HeartTransaction.ForAttendance(
                userId, Guid.NewGuid(), "Yoga", 250, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var profile = await ClientAs(userId).GetFromJsonAsync<ProfileResponse>("/api/me");

        profile!.Name.Should().Be("Alex Tremblay");
        profile.Email.Should().Be("alex@uqtr.ca");
        profile.TotalHearts.Should().Be(250);
        profile.Level.Should().Be("Argent");   // 200 <= 250 < 500
    }

    [Fact]
    public async Task Get_me_without_user_returns_401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_create_activity_appears_in_catalogue()
    {
        var categoryId = await SeedCategoryAsync();
        var admin = _factory.CreateAdminClient();

        var create = await admin.PostAsJsonAsync(
            "/api/admin/activities", ValidActivityBody(categoryId, "Nouvelle activité"));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<CreatedResponse>();

        var catalogue = await admin.GetFromJsonAsync<List<CatalogueItem>>("/api/activities");
        catalogue!.Should().Contain(a => a.Id == created!.Id && a.Title == "Nouvelle activité");
    }

    [Fact]
    public async Task Admin_create_with_unknown_category_returns_400()
    {
        var response = await _factory.CreateAdminClient().PostAsJsonAsync(
            "/api/admin/activities", ValidActivityBody(Guid.NewGuid(), "X"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Admin_update_can_cancel_activity_removing_it_from_catalogue()
    {
        var categoryId = await SeedCategoryAsync();
        var admin = _factory.CreateAdminClient();

        var create = await admin.PostAsJsonAsync(
            "/api/admin/activities", ValidActivityBody(categoryId, "À annuler"));
        var created = await create.Content.ReadFromJsonAsync<CreatedResponse>();

        // Mise à jour : passage en "cancelled" → sort du catalogue (publiées seulement).
        var update = await admin.PutAsJsonAsync(
            $"/api/admin/activities/{created!.Id}",
            ValidActivityBody(categoryId, "À annuler", status: "cancelled"));
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var catalogue = await admin.GetFromJsonAsync<List<CatalogueItem>>("/api/activities");
        catalogue!.Should().NotContain(a => a.Id == created.Id);
    }

    [Fact]
    public async Task Admin_update_unknown_activity_returns_404()
    {
        var categoryId = await SeedCategoryAsync();
        var response = await _factory.CreateAdminClient().PutAsJsonAsync(
            $"/api/admin/activities/{Guid.NewGuid()}", ValidActivityBody(categoryId, "X"));
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record ProfileResponse
    {
        public string Name { get; init; } = "";
        public string Email { get; init; } = "";
        public int TotalHearts { get; init; }
        public string Level { get; init; } = "";
    }

    private sealed record CreatedResponse
    {
        public Guid Id { get; init; }
    }

    private sealed record CatalogueItem
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = "";
    }
}
