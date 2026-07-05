using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Social;
using EventHub.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class LikePostHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _postId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IPostRepository> _posts = new();

    private LikePostHandler LikeHandler() =>
        new(_posts.Object, Mock.Of<IClock>(c => c.UtcNow == Now));

    private UnlikePostHandler UnlikeHandler() => new(_posts.Object);

    [Fact]
    public async Task Like_adds_when_not_already_liked()
    {
        _posts.Setup(p => p.ExistsAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _posts.Setup(p => p.LikeExistsAsync(_postId, _userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _posts.Setup(p => p.CountLikesAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await LikeHandler().HandleAsync(new LikePostCommand(_userId, _postId));

        result.Status.Should().Be(LikeResultStatus.Updated);
        result.LikesCount.Should().Be(1);
        _posts.Verify(p => p.AddLikeAsync(It.IsAny<PostLike>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Like_is_idempotent_when_already_liked()
    {
        _posts.Setup(p => p.ExistsAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _posts.Setup(p => p.LikeExistsAsync(_postId, _userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _posts.Setup(p => p.CountLikesAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await LikeHandler().HandleAsync(new LikePostCommand(_userId, _postId));

        result.LikesCount.Should().Be(1);
        _posts.Verify(p => p.AddLikeAsync(It.IsAny<PostLike>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Like_returns_not_found_when_post_missing()
    {
        _posts.Setup(p => p.ExistsAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await LikeHandler().HandleAsync(new LikePostCommand(_userId, _postId));

        result.Status.Should().Be(LikeResultStatus.PostNotFound);
    }

    [Fact]
    public async Task Unlike_removes_when_liked()
    {
        _posts.Setup(p => p.ExistsAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _posts.Setup(p => p.LikeExistsAsync(_postId, _userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _posts.Setup(p => p.CountLikesAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await UnlikeHandler().HandleAsync(new UnlikePostCommand(_userId, _postId));

        result.LikesCount.Should().Be(0);
        _posts.Verify(p => p.RemoveLikeAsync(_postId, _userId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class AddCommentHandlerTests
{
    private readonly Guid _postId = Guid.NewGuid();
    private readonly Mock<IPostRepository> _posts = new();

    private AddCommentHandler Handler() =>
        new(_posts.Object, Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow));

    [Fact]
    public async Task Adds_comment_when_post_exists()
    {
        _posts.Setup(p => p.ExistsAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Handler().HandleAsync(new AddCommentCommand(Guid.NewGuid(), _postId, "Bravo !"));

        result.Status.Should().Be(AddCommentStatus.Added);
        result.CommentId.Should().NotBeNull();
        _posts.Verify(p => p.AddCommentAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Returns_not_found_when_post_missing()
    {
        _posts.Setup(p => p.ExistsAsync(_postId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await Handler().HandleAsync(new AddCommentCommand(Guid.NewGuid(), _postId, "Bravo !"));

        result.Status.Should().Be(AddCommentStatus.PostNotFound);
        _posts.Verify(p => p.AddCommentAsync(It.IsAny<Comment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class CreatePostHandlerTests
{
    [Fact]
    public async Task Creates_post_and_saves()
    {
        var posts = new Mock<IPostRepository>();
        Post? captured = null;
        posts.Setup(p => p.AddAsync(It.IsAny<Post>(), It.IsAny<CancellationToken>()))
            .Callback<Post, CancellationToken>((p, _) => captured = p);

        var handler = new CreatePostHandler(
            posts.Object, Mock.Of<IClock>(c => c.UtcNow == DateTime.UtcNow));
        var authorId = Guid.NewGuid();

        var id = await handler.HandleAsync(
            new CreatePostCommand(authorId, "https://img", "Super journée", null));

        id.Should().NotBeEmpty();
        captured.Should().NotBeNull();
        captured!.AuthorId.Should().Be(authorId);
        captured.Caption.Should().Be("Super journée");
        posts.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
