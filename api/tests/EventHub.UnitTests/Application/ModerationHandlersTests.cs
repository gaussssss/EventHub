using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Moderation;
using EventHub.Application.Social;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class ReportEntityTests
{
    [Fact]
    public void New_report_is_open()
    {
        var report = Report.Create(
            Guid.NewGuid(), ReportTargetType.Post, Guid.NewGuid(), "spam", DateTime.UtcNow);
        report.Status.Should().Be(ReportStatus.Open);
    }

    [Fact]
    public void Resolve_and_dismiss_change_status()
    {
        var report = Report.Create(
            Guid.NewGuid(), ReportTargetType.Post, Guid.NewGuid(), "spam", DateTime.UtcNow);

        report.Resolve();
        report.Status.Should().Be(ReportStatus.Resolved);

        report.Dismiss();
        report.Status.Should().Be(ReportStatus.Dismissed);
    }
}

public class ReportContentHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IReportRepository> _reports = new();
    private readonly Mock<IPostRepository> _posts = new();

    private ReportContentHandler Handler() =>
        new(_reports.Object, _posts.Object, Mock.Of<IClock>(c => c.UtcNow == Now));

    [Fact]
    public async Task Creates_report_when_target_post_exists()
    {
        var postId = Guid.NewGuid();
        _posts.Setup(p => p.ExistsAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Handler().HandleAsync(
            new ReportContentCommand(Guid.NewGuid(), ReportTargetType.Post, postId, "spam"));

        result.Status.Should().Be(ReportContentStatus.Created);
        result.ReportId.Should().NotBeNull();
        _reports.Verify(r => r.AddAsync(
            It.Is<Report>(x => x.TargetId == postId && x.Reason == "spam" && x.CreatedAt == Now),
            It.IsAny<CancellationToken>()), Times.Once);
        _reports.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Uses_comment_existence_for_comment_target()
    {
        var commentId = Guid.NewGuid();
        _posts.Setup(p => p.CommentExistsAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Handler().HandleAsync(
            new ReportContentCommand(Guid.NewGuid(), ReportTargetType.Comment, commentId, "abus"));

        result.Status.Should().Be(ReportContentStatus.Created);
        _posts.Verify(p => p.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Fails_when_target_missing()
    {
        _posts.Setup(p => p.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await Handler().HandleAsync(
            new ReportContentCommand(Guid.NewGuid(), ReportTargetType.Post, Guid.NewGuid(), "spam"));

        result.Status.Should().Be(ReportContentStatus.TargetNotFound);
        _reports.Verify(r => r.AddAsync(It.IsAny<Report>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class HideContentHandlerTests
{
    private readonly Mock<IPostRepository> _posts = new();
    private readonly Mock<IReportRepository> _reports = new();

    private HidePostHandler PostHandler() => new(_posts.Object, _reports.Object, Mock.Of<IClock>());
    private HideCommentHandler CommentHandler() => new(_posts.Object, _reports.Object, Mock.Of<IClock>());

    [Fact]
    public async Task Hiding_post_sets_status_and_resolves_open_reports()
    {
        var postId = Guid.NewGuid();
        var post = Post.Create(Guid.NewGuid(), "i", "c", null, DateTime.UtcNow);
        var openReport = Report.Create(
            Guid.NewGuid(), ReportTargetType.Post, postId, "spam", DateTime.UtcNow);
        _posts.Setup(p => p.GetPostAsync(postId, It.IsAny<CancellationToken>())).ReturnsAsync(post);
        _reports.Setup(r => r.GetOpenForTargetAsync(
                ReportTargetType.Post, postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { openReport });

        var result = await PostHandler().HandleAsync(new HidePostCommand(postId));

        result.Should().Be(HideResult.Hidden);
        post.Status.Should().Be("hidden");
        openReport.Status.Should().Be(ReportStatus.Resolved);
        _posts.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Hiding_missing_post_returns_not_found()
    {
        _posts.Setup(p => p.GetPostAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post?)null);

        var result = await PostHandler().HandleAsync(new HidePostCommand(Guid.NewGuid()));

        result.Should().Be(HideResult.NotFound);
        _posts.Verify(p => p.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Hiding_comment_sets_status_hidden()
    {
        var commentId = Guid.NewGuid();
        var comment = Comment.Create(Guid.NewGuid(), Guid.NewGuid(), "t", DateTime.UtcNow);
        _posts.Setup(p => p.GetCommentAsync(commentId, It.IsAny<CancellationToken>())).ReturnsAsync(comment);
        _reports.Setup(r => r.GetOpenForTargetAsync(
                ReportTargetType.Comment, commentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Report>());

        var result = await CommentHandler().HandleAsync(new HideCommentCommand(commentId));

        result.Should().Be(HideResult.Hidden);
        comment.Status.Should().Be("hidden");
    }
}
