using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class HealthEndpointTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public HealthEndpointTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_health_returns_ok_with_version()
    {
        var response = await _factory.CreateClient().GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<HealthRow>();
        payload!.Status.Should().Be("ok");
        payload.Version.Should().NotBeNullOrEmpty();
    }

    private sealed record HealthRow
    {
        public string Status { get; init; } = "";
        public string Version { get; init; } = "";
    }
}
