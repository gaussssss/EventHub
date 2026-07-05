using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class AdminReferentialsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public AdminReferentialsTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Category_crud_lifecycle()
    {
        var admin = _factory.CreateAdminClient();
        var slug = $"slug-{Guid.NewGuid():N}";

        var create = await admin.PostAsJsonAsync("/api/admin/categories",
            new { slug, label = "Bien-être", color = "#22c55e", icon = "heart" });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await create.Content.ReadFromJsonAsync<Created>())!.Id;

        // Doublon de slug → 409
        var dup = await admin.PostAsJsonAsync("/api/admin/categories",
            new { slug, label = "Autre" });
        dup.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // PATCH
        var update = await admin.PatchAsJsonAsync($"/api/admin/categories/{id}",
            new { slug, label = "Santé & bien-être", color = (string?)null, icon = (string?)null });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await admin.GetFromJsonAsync<List<CategoryRow>>("/api/categories"))!
            .Should().Contain(c => c.Slug == slug && c.Label == "Santé & bien-être");

        // DELETE
        (await admin.DeleteAsync($"/api/admin/categories/{id}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await admin.GetFromJsonAsync<List<CategoryRow>>("/api/categories"))!
            .Should().NotContain(c => c.Slug == slug);
    }

    [Fact]
    public async Task Organizer_crud_lifecycle()
    {
        var admin = _factory.CreateAdminClient();

        var create = await admin.PostAsJsonAsync("/api/admin/organizers",
            new { name = "Club de course UQTR", contactEmail = "course@uqtr.ca" });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = (await create.Content.ReadFromJsonAsync<Created>())!.Id;

        var update = await admin.PatchAsJsonAsync($"/api/admin/organizers/{id}",
            new { name = "Club d'athlétisme UQTR", contactEmail = (string?)null });
        update.StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await admin.GetFromJsonAsync<List<OrganizerRow>>("/api/admin/organizers"))!
            .Should().Contain(o => o.Id == id && o.Name == "Club d'athlétisme UQTR");

        (await admin.DeleteAsync($"/api/admin/organizers/{id}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Update_unknown_category_returns_404()
    {
        var response = await _factory.CreateAdminClient().PatchAsJsonAsync(
            $"/api/admin/categories/{Guid.NewGuid()}", new { slug = "x", label = "X" });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record Created { public Guid Id { get; init; } }
    private sealed record CategoryRow { public string Slug { get; init; } = ""; public string Label { get; init; } = ""; }
    private sealed record OrganizerRow { public Guid Id { get; init; } public string Name { get; init; } = ""; }
}
