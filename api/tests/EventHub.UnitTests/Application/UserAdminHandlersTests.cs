using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Hearts;
using EventHub.Application.Users;
using EventHub.Domain;
using EventHub.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class UserRolesTests
{
    [Theory]
    [InlineData("student")]
    [InlineData("ORGANIZER")]
    [InlineData("Moderator")]
    [InlineData("admin")]
    public void Recognizes_known_roles_ignoring_case(string role) =>
        UserRoles.IsValid(role).Should().BeTrue();

    [Theory]
    [InlineData("superuser")]
    [InlineData("")]
    [InlineData(null)]
    public void Rejects_unknown_roles(string? role) =>
        UserRoles.IsValid(role).Should().BeFalse();

    [Fact]
    public void Normalize_lowercases_valid_role_and_nulls_invalid()
    {
        UserRoles.Normalize("ADMIN").Should().Be("admin");
        UserRoles.Normalize("nope").Should().BeNull();
    }
}

public class UpdateUserHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IUserAdminService> _users = new();

    private UpdateUserHandler Handler() => new(_users.Object);

    [Fact]
    public async Task Sets_role_and_status_when_valid()
    {
        _users.Setup(u => u.ExistsAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var status = await Handler().HandleAsync(new UpdateUserCommand(_userId, "Organizer", "suspended"));

        status.Should().Be(UpdateUserStatus.Updated);
        _users.Verify(u => u.SetRoleAsync(_userId, "organizer", It.IsAny<CancellationToken>()), Times.Once);
        _users.Verify(u => u.SetStatusAsync(_userId, "suspended", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rejects_invalid_role_without_touching_user()
    {
        var status = await Handler().HandleAsync(new UpdateUserCommand(_userId, "wizard", null));

        status.Should().Be(UpdateUserStatus.InvalidRole);
        _users.Verify(u => u.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Rejects_invalid_status()
    {
        var status = await Handler().HandleAsync(new UpdateUserCommand(_userId, null, "banned"));
        status.Should().Be(UpdateUserStatus.InvalidStatus);
    }

    [Fact]
    public async Task Returns_not_found_when_user_absent()
    {
        _users.Setup(u => u.ExistsAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var status = await Handler().HandleAsync(new UpdateUserCommand(_userId, "admin", null));

        status.Should().Be(UpdateUserStatus.NotFound);
        _users.Verify(u => u.SetRoleAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class AdjustHeartsHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IUserAdminService> _users = new();
    private readonly Mock<IHeartTransactionRepository> _tx = new();
    private readonly Mock<IHeartReadRepository> _read = new();

    private AdjustHeartsHandler Handler() =>
        new(_users.Object, _tx.Object, _read.Object, Mock.Of<IClock>(c => c.UtcNow == Now));

    [Fact]
    public async Task Records_transaction_and_returns_new_total()
    {
        _users.Setup(u => u.ExistsAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _read.Setup(r => r.GetTotalAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(120);

        var result = await Handler().HandleAsync(new AdjustHeartsCommand(_userId, -30, "sanction"));

        result.Status.Should().Be(AdjustHeartsStatus.Adjusted);
        result.NewTotal.Should().Be(120);
        _tx.Verify(t => t.AddAsync(
            It.Is<HeartTransaction>(h => h.Hearts == -30 && h.Reason == "sanction" && h.UserId == _userId),
            It.IsAny<CancellationToken>()), Times.Once);
        _tx.Verify(t => t.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rejects_zero_amount()
    {
        var result = await Handler().HandleAsync(new AdjustHeartsCommand(_userId, 0, "noop"));
        result.Status.Should().Be(AdjustHeartsStatus.InvalidAmount);
        _tx.Verify(t => t.AddAsync(It.IsAny<HeartTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_not_found_when_user_absent()
    {
        _users.Setup(u => u.ExistsAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await Handler().HandleAsync(new AdjustHeartsCommand(_userId, 50, "bonus"));

        result.Status.Should().Be(AdjustHeartsStatus.UserNotFound);
        _tx.Verify(t => t.AddAsync(It.IsAny<HeartTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
