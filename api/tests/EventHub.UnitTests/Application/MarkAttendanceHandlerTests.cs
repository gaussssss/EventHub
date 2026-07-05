using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Attendance;
using EventHub.Application.Hearts;
using EventHub.Application.Registrations;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class MarkAttendanceHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _activityId = Guid.NewGuid();

    private readonly Mock<IActivityRepository> _activities = new();
    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<IHeartTransactionRepository> _hearts = new();

    private MarkAttendanceHandler BuildHandler() => new(
        _activities.Object, _registrations.Object, _hearts.Object,
        Mock.Of<IClock>(c => c.UtcNow == Now));

    private void GivenActivity(int heartsReward = 40) =>
        _activities.Setup(r => r.GetAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Activity.Create(
                "Course en forêt", "…", Guid.NewGuid(), null, Now, null, "Parc", "https://img",
                heartsReward, 40, null, null, isFeatured: false, ActivityStatus.Published, Now));

    private void GivenRegistrations(params Registration[] registrations) =>
        _registrations.Setup(r => r.GetForActivityAsync(
                _activityId, It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(registrations);

    private static Registration Reg(Guid userId, RegistrationStatus status = RegistrationStatus.Registered)
        => Registration.Create(userId, Guid.NewGuid(), status, Registration.SourceApp, null, Now);

    [Fact]
    public async Task Credits_hearts_and_marks_attended_for_registered_users()
    {
        GivenActivity(heartsReward: 40);
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        var regA = Reg(userA);
        var regB = Reg(userB);
        GivenRegistrations(regA, regB);

        var result = await BuildHandler().HandleAsync(
            new MarkAttendanceCommand(_activityId, new[] { userA, userB }));

        result.Status.Should().Be(AttendanceResultStatus.Ok);
        result.Credited.Should().Be(2);
        regA.Status.Should().Be(RegistrationStatus.Attended);
        regA.AttendedAt.Should().Be(Now);
        _hearts.Verify(h => h.AddAsync(
            It.Is<HeartTransaction>(t => t.Hearts == 40 && t.Reason == "attendance"),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
        _hearts.Verify(h => h.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Does_not_double_credit_already_credited_users()
    {
        GivenActivity(heartsReward: 40);
        var user = Guid.NewGuid();
        GivenRegistrations(Reg(user));
        _hearts.Setup(h => h.HasAttendanceCreditAsync(
                user, _activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await BuildHandler().HandleAsync(
            new MarkAttendanceCommand(_activityId, new[] { user }));

        result.Credited.Should().Be(0);
        _hearts.Verify(h => h.AddAsync(
            It.IsAny<HeartTransaction>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Skips_cancelled_registrations()
    {
        GivenActivity();
        var user = Guid.NewGuid();
        var reg = Reg(user, RegistrationStatus.Cancelled);
        GivenRegistrations(reg);

        var result = await BuildHandler().HandleAsync(
            new MarkAttendanceCommand(_activityId, new[] { user }));

        result.Credited.Should().Be(0);
        reg.Status.Should().Be(RegistrationStatus.Cancelled);
    }

    [Fact]
    public async Task Returns_not_found_when_activity_missing()
    {
        _activities.Setup(r => r.GetAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await BuildHandler().HandleAsync(
            new MarkAttendanceCommand(_activityId, new[] { Guid.NewGuid() }));

        result.Status.Should().Be(AttendanceResultStatus.ActivityNotFound);
    }
}
