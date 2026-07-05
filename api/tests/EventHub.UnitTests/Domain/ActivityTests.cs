using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.UnitTests.Domain;

public class ActivityTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Activity NewActivity(
        ActivityStatus status = ActivityStatus.Published,
        int maxParticipants = 10,
        DateTime? deadline = null) => Activity.Create(
        "Course en forêt", "…", Guid.NewGuid(), null, Now.AddDays(7), null,
        "Parc portuaire", "https://img", 0, maxParticipants, null, deadline,
        isFeatured: false, status, Now);

    [Fact]
    public void IsRegistrationOpen_is_true_when_published_and_no_deadline()
    {
        NewActivity().IsRegistrationOpen(Now).Should().BeTrue();
    }

    [Fact]
    public void IsRegistrationOpen_is_false_when_deadline_passed()
    {
        var activity = NewActivity(deadline: Now.AddDays(-1));
        activity.IsRegistrationOpen(Now).Should().BeFalse();
    }

    [Fact]
    public void IsRegistrationOpen_is_false_when_not_published()
    {
        NewActivity(status: ActivityStatus.Draft).IsRegistrationOpen(Now).Should().BeFalse();
    }

    [Fact]
    public void DetermineOutcome_registers_when_capacity_available()
    {
        var outcome = NewActivity(maxParticipants: 10).DetermineOutcome(5, Now);

        outcome.IsAccepted.Should().BeTrue();
        outcome.Status.Should().Be(RegistrationStatus.Registered);
    }

    [Fact]
    public void DetermineOutcome_waitlists_when_full()
    {
        var outcome = NewActivity(maxParticipants: 10).DetermineOutcome(10, Now);

        outcome.IsAccepted.Should().BeTrue();
        outcome.Status.Should().Be(RegistrationStatus.Waitlisted);
    }

    [Fact]
    public void DetermineOutcome_rejects_when_deadline_passed()
    {
        var outcome = NewActivity(deadline: Now.AddDays(-1)).DetermineOutcome(0, Now);

        outcome.IsAccepted.Should().BeFalse();
        outcome.Reason.Should().Be(RegistrationRejectionReason.DeadlinePassed);
    }

    [Fact]
    public void DetermineOutcome_rejects_when_not_published()
    {
        var outcome = NewActivity(status: ActivityStatus.Cancelled).DetermineOutcome(0, Now);

        outcome.IsAccepted.Should().BeFalse();
        outcome.Reason.Should().Be(RegistrationRejectionReason.NotPublished);
    }
}
