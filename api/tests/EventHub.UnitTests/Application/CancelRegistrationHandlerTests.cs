using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Registrations;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class CancelRegistrationHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _activityId = Guid.NewGuid();

    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<IRealtimeNotifier> _notifier = new();

    private CancelRegistrationHandler BuildHandler() => new(
        _registrations.Object, _notifier.Object,
        Mock.Of<IClock>(c => c.UtcNow == Now));

    private void GivenExisting(RegistrationStatus status) =>
        _registrations.Setup(r => r.FindAsync(_userId, _activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Registration.Create(
                _userId, _activityId, status, Registration.SourceApp, null, Now));

    [Fact]
    public async Task Cancels_and_promotes_first_waitlisted()
    {
        GivenExisting(RegistrationStatus.Registered);
        var waitlisted = Registration.Create(
            Guid.NewGuid(), _activityId, RegistrationStatus.Waitlisted,
            Registration.SourceApp, null, Now);
        _registrations.Setup(r => r.FindFirstWaitlistedAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(waitlisted);
        _registrations.Setup(r => r.CountActiveAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await BuildHandler().HandleAsync(new CancelRegistrationCommand(_userId, _activityId));

        result.Status.Should().Be(CancellationResultStatus.Cancelled);
        result.PromotedUserId.Should().Be(waitlisted.UserId);
        waitlisted.Status.Should().Be(RegistrationStatus.Registered);
        _notifier.Verify(n => n.ActivityParticipantsChangedAsync(
            _activityId, 1, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.RegistrationPromotedAsync(
            waitlisted.UserId, _activityId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Cancels_without_promotion_when_no_waitlist()
    {
        GivenExisting(RegistrationStatus.Registered);
        _registrations.Setup(r => r.FindFirstWaitlistedAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);
        _registrations.Setup(r => r.CountActiveAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await BuildHandler().HandleAsync(new CancelRegistrationCommand(_userId, _activityId));

        result.PromotedUserId.Should().BeNull();
        _notifier.Verify(n => n.ActivityParticipantsChangedAsync(
            _activityId, 0, It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.RegistrationPromotedAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancelling_a_waitlisted_spot_does_not_promote_or_notify()
    {
        GivenExisting(RegistrationStatus.Waitlisted);

        var result = await BuildHandler().HandleAsync(new CancelRegistrationCommand(_userId, _activityId));

        result.Status.Should().Be(CancellationResultStatus.Cancelled);
        result.PromotedUserId.Should().BeNull();
        _registrations.Verify(r => r.FindFirstWaitlistedAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _notifier.Verify(n => n.ActivityParticipantsChangedAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_not_registered_when_no_active_registration()
    {
        _registrations.Setup(r => r.FindAsync(_userId, _activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Registration?)null);

        var result = await BuildHandler().HandleAsync(new CancelRegistrationCommand(_userId, _activityId));

        result.Status.Should().Be(CancellationResultStatus.NotRegistered);
    }

    [Fact]
    public async Task Returns_not_registered_when_already_cancelled()
    {
        GivenExisting(RegistrationStatus.Cancelled);

        var result = await BuildHandler().HandleAsync(new CancelRegistrationCommand(_userId, _activityId));

        result.Status.Should().Be(CancellationResultStatus.NotRegistered);
    }
}
