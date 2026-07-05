using System.Net;
using System.Net.Http.Json;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class ModerationEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public ModerationEndpointsTests(EventHubApiFactory factory) => _factory = factory;

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

    private async Task<Guid> CreatePostAsync(HttpClient client, string caption = "Photo")
    {
        var response = await client.PostAsJsonAsync("/api/posts",
            new { imageUrl = "https://img/photo.jpg", caption });
        return (await response.Content.ReadFromJsonAsync<IdResponse>())!.Id;
    }

    [Fact]
    public async Task Report_post_appears_in_admin_queue()
    {
        var author = await SeedUserAsync("Auteur");
        var reporter = await SeedUserAsync("Sonia Lévesque");
        var postId = await CreatePostAsync(ClientAs(author));

        var report = await ClientAs(reporter).PostAsJsonAsync("/api/reports",
            new { targetType = "post", targetId = postId, reason = "spam" });
        report.StatusCode.Should().Be(HttpStatusCode.Created);

        var queue = await _factory.CreateAdminClient().GetFromJsonAsync<List<ReportResponse>>("/api/admin/reports");
        queue!.Should().Contain(r =>
            r.TargetId == postId && r.Reason == "spam"
            && r.Status == "Open" && r.ReporterName == "Sonia Lévesque");
    }

    [Fact]
    public async Task Hiding_post_removes_it_from_feed_and_closes_reports()
    {
        var author = await SeedUserAsync("Auteur");
        var reporter = await SeedUserAsync("Reporter");
        var postId = await CreatePostAsync(ClientAs(author), "À masquer");

        await ClientAs(reporter).PostAsJsonAsync("/api/reports",
            new { targetType = "post", targetId = postId, reason = "abus" });

        var admin = _factory.CreateAdminClient();
        var hide = await admin.PostAsync($"/api/admin/posts/{postId}/hide", null);
        hide.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var feed = await admin.GetFromJsonAsync<List<IdResponse>>("/api/posts");
        feed!.Should().NotContain(p => p.Id == postId);

        // Le signalement associé est clôturé → sort de la file ouverte.
        var queue = await admin.GetFromJsonAsync<List<ReportResponse>>("/api/admin/reports");
        queue!.Should().NotContain(r => r.TargetId == postId);
    }

    [Fact]
    public async Task Hiding_comment_removes_it_from_post_detail()
    {
        var author = await SeedUserAsync("Auteur");
        var commenter = await SeedUserAsync("Camille");
        var postId = await CreatePostAsync(ClientAs(author));

        var comment = await ClientAs(commenter).PostAsJsonAsync(
            $"/api/posts/{postId}/comments", new { text = "Contenu inapproprié" });
        var commentId = (await comment.Content.ReadFromJsonAsync<CommentIdResponse>())!.CommentId;

        var admin = _factory.CreateAdminClient();
        var hide = await admin.PostAsync($"/api/admin/comments/{commentId}/hide", null);
        hide.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var detail = await admin.GetFromJsonAsync<PostDetailResponse>($"/api/posts/{postId}");
        detail!.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Report_unknown_target_returns_404()
    {
        var reporter = await SeedUserAsync("Reporter");
        var response = await ClientAs(reporter).PostAsJsonAsync("/api/reports",
            new { targetType = "post", targetId = Guid.NewGuid(), reason = "x" });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Report_without_user_returns_401()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/reports",
            new { targetType = "post", targetId = Guid.NewGuid(), reason = "x" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Hiding_unknown_post_returns_404()
    {
        var response = await _factory.CreateAdminClient()
            .PostAsync($"/api/admin/posts/{Guid.NewGuid()}/hide", null);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private sealed record IdResponse
    {
        public Guid Id { get; init; }
    }

    private sealed record CommentIdResponse
    {
        public Guid CommentId { get; init; }
    }

    private sealed record ReportResponse
    {
        public Guid TargetId { get; init; }
        public string Reason { get; init; } = "";
        public string Status { get; init; } = "";
        public string ReporterName { get; init; } = "";
    }

    private sealed record PostDetailResponse
    {
        public Guid Id { get; init; }
        public List<CommentResponse> Comments { get; init; } = new();
    }

    private sealed record CommentResponse
    {
        public string Text { get; init; } = "";
    }
}
