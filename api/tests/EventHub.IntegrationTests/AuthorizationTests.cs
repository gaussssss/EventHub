using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

/// <summary>
/// Vérifie le contrôle d'accès via le schéma « Dev » (X-User-Id / X-User-Roles),
/// qui reproduit le comportement de l'auth Entra une fois activée.
/// </summary>
public class AuthorizationTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public AuthorizationTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Protected_me_route_without_identity_returns_401()
    {
        var response = await _factory.CreateClient().GetAsync("/api/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_route_with_non_admin_role_returns_403()
    {
        var student = _factory.CreateClientAs(Guid.NewGuid(), "student");
        var response = await student.PostAsJsonAsync("/api/admin/categories",
            new { slug = $"s-{Guid.NewGuid():N}", label = "X" });
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_route_with_admin_role_succeeds()
    {
        var admin = _factory.CreateClientAs(Guid.NewGuid(), "admin");
        var response = await admin.PostAsJsonAsync("/api/admin/categories",
            new { slug = $"s-{Guid.NewGuid():N}", label = "OK" });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Public_catalogue_route_stays_open()
    {
        var response = await _factory.CreateClient().GetAsync("/api/activities");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
