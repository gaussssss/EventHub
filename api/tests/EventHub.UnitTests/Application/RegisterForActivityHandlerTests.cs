using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Common.Exceptions;
using EventHub.Application.Registrations;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class RegisterForActivityHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _activityId = Guid.NewGuid();

    private readonly Mock<IActivityRepository> _activities = new();
    private readonly Mock<IRegistrationRepository> _registrations = new();
    private readonly Mock<IRealtimeNotifier> _notifier = new();

    private RegisterForActivityHandler BuildHandler() => new(
        _activities.Object, _registrations.Object, _notifier.Object,
        Mock.Of<IClock>(c => c.UtcNow == Now));

    private Activity GivenActivity(
        int max = 10, ActivityStatus status = ActivityStatus.Published, DateTime? deadline = null)
    {
        var activity = Activity.Create(
            "Course", "…", Guid.NewGuid(), null, Now.AddDays(5), null, "Parc", "https://img",
            0, max, null, deadline, isFeatured: false, status, Now);
        _activities.Setup(r => r.GetAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);
        return activity;
    }

    [Fact]
    public async Task Registers_and_notifies_when_capacity_available()
    {
        GivenActivity(max: 10);
        _registrations.Setup(r => r.CountActiveAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        result.Status.Should().Be(RegistrationResultStatus.Registered);
        _registrations.Verify(r => r.AddAsync(
            It.Is<Registration>(x => x.Status == RegistrationStatus.Registered),
            It.IsAny<CancellationToken>()), Times.Once);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.ActivityParticipantsChangedAsync(
            _activityId, 6, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Waitlists_without_notifying_when_full()
    {
        GivenActivity(max: 10);
        _registrations.Setup(r => r.CountActiveAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var result = await BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        result.Status.Should().Be(RegistrationResultStatus.Waitlisted);
        _notifier.Verify(n => n.ActivityParticipantsChangedAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Rejects_when_deadline_passed()
    {
        GivenActivity(deadline: Now.AddDays(-1));

        var result = await BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        result.Status.Should().Be(RegistrationResultStatus.Rejected);
        result.Reason.Should().Be(RegistrationRejectionReason.DeadlinePassed);
        _registrations.Verify(r => r.AddAsync(
            It.IsAny<Registration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Is_idempotent_when_already_registered()
    {
        GivenActivity();
        _registrations.Setup(r => r.FindAsync(_userId, _activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Registration.Create(
                _userId, _activityId, RegistrationStatus.Registered,
                Registration.SourceApp, null, Now));

        var result = await BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        result.Status.Should().Be(RegistrationResultStatus.AlreadyRegistered);
        _registrations.Verify(r => r.AddAsync(
            It.IsAny<Registration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_not_found_when_activity_missing()
    {
        _activities.Setup(r => r.GetAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        result.Status.Should().Be(RegistrationResultStatus.ActivityNotFound);
    }

    [Fact]
    public async Task Retries_and_waitlists_when_last_spot_is_taken_concurrently()
    {
        // Activité d'1 place. 1ère évaluation : libre → tentative « inscrit ».
        // La sauvegarde échoue (un concurrent a pris la place) → rejeu : plus de
        // place → liste d'attente, sans jamais sur-réserver.
        GivenActivity(max: 1);
        _registrations.SetupSequence(r => r.CountActiveAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0)   // 1er passage : place libre
            .ReturnsAsync(1);  // rejeu : complet
        _registrations.SetupSequence(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException())  // conflit sur la dernière place
            .Returns(Task.CompletedTask);                     // rejeu : succès

        var result = await BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        result.Status.Should().Be(RegistrationResultStatus.Waitlisted);
        // L'inscription n'est ajoutée qu'une fois malgré le rejeu.
        _registrations.Verify(r => r.AddAsync(
            It.IsAny<Registration>(), It.IsAny<CancellationToken>()), Times.Once);
        _registrations.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        _notifier.Verify(n => n.ActivityParticipantsChangedAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Propagates_conflict_after_exhausting_retries()
    {
        GivenActivity(max: 5);
        _registrations.Setup(r => r.CountActiveAsync(_activityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _registrations.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConcurrencyConflictException());

        var act = () => BuildHandler().HandleAsync(new RegisterForActivityCommand(_userId, _activityId));

        await act.Should().ThrowAsync<ConcurrencyConflictException>();
    }
}
