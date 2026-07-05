using System.Net;
using System.Net.Http.Json;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class PostsEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public PostsEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    private async Task<Guid> SeedUserAsync(string name)
    {
        var userId = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser
            {
                Id = userId,
                UserName = $"user-{userId:N}",
                Email = $"{userId:N}@uqtr.ca",
                Name = name,
                AvatarUrl = "https://img/avatar.jpg",
            });
            await db.SaveChangesAsync();
        });
        return userId;
    }

    private HttpClient ClientAs(Guid userId)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        return client;
    }

    private async Task<Guid> CreatePostAsync(HttpClient client, string caption)
    {
        var response = await client.PostAsJsonAsync("/api/posts",
            new { imageUrl = "https://img/photo.jpg", caption });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>();
        return created!.Id;
    }

    [Fact]
    public async Task Create_post_appears_in_feed_with_author_name()
    {
        var userId = await SeedUserAsync("Alex Tremblay");
        var client = ClientAs(userId);

        var postId = await CreatePostAsync(client, "Super tournoi aujourd'hui !");

        var feed = await client.GetFromJsonAsync<List<PostResponse>>("/api/posts");
        var post = feed!.Single(p => p.Id == postId);
        post.AuthorName.Should().Be("Alex Tremblay");
        post.Caption.Should().Be("Super tournoi aujourd'hui !");
        post.LikesCount.Should().Be(0);
    }

    [Fact]
    public async Task Like_then_unlike_updates_count()
    {
        var author = await SeedUserAsync("Auteur");
        var liker = await SeedUserAsync("Aimeur");
        var postId = await CreatePostAsync(ClientAs(author), "Photo");

        var likeClient = ClientAs(liker);
        var like = await likeClient.PostAsync($"/api/posts/{postId}/like", null);
        (await like.Content.ReadAsStringAsync()).Should().Contain("\"likesCount\":1");

        // Idempotent : aimer deux fois reste à 1.
        var likeAgain = await likeClient.PostAsync($"/api/posts/{postId}/like", null);
        (await likeAgain.Content.ReadAsStringAsync()).Should().Contain("\"likesCount\":1");

        var unlike = await likeClient.DeleteAsync($"/api/posts/{postId}/like");
        (await unlike.Content.ReadAsStringAsync()).Should().Contain("\"likesCount\":0");
    }

    [Fact]
    public async Task Comment_appears_in_post_detail()
    {
        var author = await SeedUserAsync("Auteur");
        var commenter = await SeedUserAsync("Camille Roy");
        var postId = await CreatePostAsync(ClientAs(author), "Belle sortie");

        var comment = await ClientAs(commenter).PostAsJsonAsync(
            $"/api/posts/{postId}/comments", new { text = "Trop cool !" });
        comment.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await ClientAs(author).GetFromJsonAsync<PostResponse>($"/api/posts/{postId}");
        detail!.Comments.Should().ContainSingle(c =>
            c.AuthorName == "Camille Roy" && c.Text == "Trop cool !");
    }

    [Fact]
    public async Task Like_unknown_post_returns_404()
    {
        var response = await ClientAs(Guid.NewGuid())
            .PostAsync($"/api/posts/{Guid.NewGuid()}/like", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_post_without_user_returns_401()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/posts",
            new { imageUrl = "https://img", caption = "x" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record CreatedResponse
    {
        public Guid Id { get; init; }
    }

    private sealed record PostResponse
    {
        public Guid Id { get; init; }
        public string AuthorName { get; init; } = "";
        public string Caption { get; init; } = "";
        public int LikesCount { get; init; }
        public List<CommentResponse> Comments { get; init; } = new();
    }

    private sealed record CommentResponse
    {
        public string AuthorName { get; init; } = "";
        public string Text { get; init; } = "";
    }
}
