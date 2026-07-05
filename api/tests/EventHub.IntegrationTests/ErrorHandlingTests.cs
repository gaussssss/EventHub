using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class ErrorHandlingTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public ErrorHandlingTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Domain_invariant_violation_becomes_problem_details_400()
    {
        Guid categoryId = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var category = Category.Create($"cat-{Guid.NewGuid():N}", "Cat");
            categoryId = category.Id;
            db.Categories.Add(category);
            await db.SaveChangesAsync();
        });

        // maxParticipants = 0 → Guard.AgainstNonPositive → DomainException non gérée
        // dans le handler → interceptée par GlobalExceptionHandler → 400 ProblemDetails.
        var response = await _factory.CreateAdminClient().PostAsJsonAsync("/api/admin/activities", new
        {
            title = "X",
            description = "Y",
            categoryId,
            startsAt = new DateTime(2027, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            location = "L",
            imageUrl = "https://img/a.jpg",
            heartsReward = 10,
            maxParticipants = 0,
            isFeatured = false,
            status = "published",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }
}
