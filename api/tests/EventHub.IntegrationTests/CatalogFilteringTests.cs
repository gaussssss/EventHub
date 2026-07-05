using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class CatalogFilteringTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public CatalogFilteringTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_activities_filters_by_category_search_and_dates()
    {
        Guid sportId = Guid.Empty, cultureId = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var sport = Category.Create("sport-filtre", "Sport");
            var culture = Category.Create("culture-filtre", "Culture");
            db.Categories.AddRange(sport, culture);

            var basket = Activity.Create(
                "Tournoi de basket", "3 contre 3", sport.Id, null,
                new DateTime(2026, 8, 10, 18, 0, 0, DateTimeKind.Utc), null,
                "Complexe sportif", "https://img/b.jpg", 50, 60, null, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            var expo = Activity.Create(
                "Exposition photo", "Vernissage", culture.Id, null,
                new DateTime(2026, 9, 1, 18, 0, 0, DateTimeKind.Utc), null,
                "Galerie", "https://img/e.jpg", 10, 40, null, null,
                false, ActivityStatus.Published, DateTime.UtcNow);
            sportId = basket.Id;
            cultureId = expo.Id;
            db.Activities.AddRange(basket, expo);
            await db.SaveChangesAsync();
        });

        var client = _factory.CreateClient();

        // Filtre catégorie
        var byCategory = await client.GetFromJsonAsync<List<Row>>(
            "/api/activities?category=sport-filtre");
        byCategory!.Select(a => a.Id).Should().Contain(sportId).And.NotContain(cultureId);

        // Recherche titre
        var bySearch = await client.GetFromJsonAsync<List<Row>>(
            "/api/activities?q=exposition");
        bySearch!.Select(a => a.Id).Should().Contain(cultureId).And.NotContain(sportId);

        // Intervalle de dates (seule l'expo de septembre)
        var byDate = await client.GetFromJsonAsync<List<Row>>(
            "/api/activities?from=2026-08-20T00:00:00Z");
        byDate!.Select(a => a.Id).Should().Contain(cultureId).And.NotContain(sportId);
    }

    [Fact]
    public async Task GET_categories_returns_referential()
    {
        await _factory.WithDbAsync(async db =>
        {
            db.Categories.Add(Category.Create("sante", "Santé", "#22c55e", "heart"));
            await db.SaveChangesAsync();
        });

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/categories");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var categories = await response.Content.ReadFromJsonAsync<List<CategoryRow>>();
        categories!.Should().Contain(c => c.Slug == "sante" && c.Label == "Santé");
    }

    private sealed record Row
    {
        public Guid Id { get; init; }
    }

    private sealed record CategoryRow
    {
        public string Slug { get; init; } = "";
        public string Label { get; init; } = "";
    }
}
