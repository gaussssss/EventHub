using System.Net;
using System.Net.Http.Json;
using EventHub.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class PostDeletionTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public PostDeletionTests(EventHubApiFactory factory) => _factory = factory;

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private async Task<Guid> SeedPostAsync(Guid authorId)
    {
        var id = Guid.Empty;
        await _factory.WithDbAsync(async db =>
        {
            var post = Post.Create(authorId, "https://img/p.jpg", "Ma photo", null, DateTime.UtcNow);
            id = post.Id;
            db.Posts.Add(post);
            await db.SaveChangesAsync();
        });
        return id;
    }

    [Fact]
    public async Task Author_can_delete_and_post_leaves_feed()
    {
        var author = Guid.NewGuid();
        var postId = await SeedPostAsync(author);

        (await ClientAs(author).DeleteAsync($"/api/posts/{postId}"))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await _factory.CreateClient().GetFromJsonAsync<List<Row>>("/api/posts"))!
            .Should().NotContain(p => p.Id == postId);
    }

    [Fact]
    public async Task Non_author_gets_403()
    {
        var postId = await SeedPostAsync(Guid.NewGuid());
        (await ClientAs(Guid.NewGuid()).DeleteAsync($"/api/posts/{postId}"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_unknown_returns_404()
    {
        (await ClientAs(Guid.NewGuid()).DeleteAsync($"/api/posts/{Guid.NewGuid()}"))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record Row { public Guid Id { get; init; } }
}
