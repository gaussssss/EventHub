using EventHub.Domain.Common;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace EventHub.UnitTests.Domain;

/// <summary>
/// Vérifie que les agrégats refusent tout état invalide (fabriques + gardes) et
/// que leurs transitions respectent leurs invariants.
/// </summary>
public class DomainInvariantsTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Activity_Create_rejects_blank_title()
    {
        var act = () => Activity.Create(
            " ", "desc", Guid.NewGuid(), null, Now, null, "lieu", "img",
            10, 20, null, null, false, ActivityStatus.Published, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Activity_Create_rejects_non_positive_capacity()
    {
        var act = () => Activity.Create(
            "Titre", "desc", Guid.NewGuid(), null, Now, null, "lieu", "img",
            10, 0, null, null, false, ActivityStatus.Published, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void HeartTransaction_ForAttendance_rejects_non_positive_amount()
    {
        var act = () => HeartTransaction.ForAttendance(Guid.NewGuid(), Guid.NewGuid(), "Yoga", 0, Now);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void HeartTransaction_ForAdjustment_rejects_zero_amount()
    {
        var act = () => HeartTransaction.ForAdjustment(Guid.NewGuid(), 0, "bonus", Now);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void HeartTransaction_ForAdjustment_allows_negative_amount()
    {
        var tx = HeartTransaction.ForAdjustment(Guid.NewGuid(), -50, "sanction", Now);
        tx.Hearts.Should().Be(-50);
    }

    [Fact]
    public void Registration_PromoteFromWaitlist_requires_waitlisted_state()
    {
        var registered = Registration.Create(
            Guid.NewGuid(), Guid.NewGuid(), RegistrationStatus.Registered,
            Registration.SourceApp, null, Now);

        var act = () => registered.PromoteFromWaitlist(Now);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Registration_MarkAttended_sets_status_and_timestamp()
    {
        var reg = Registration.Create(
            Guid.NewGuid(), Guid.NewGuid(), RegistrationStatus.Registered,
            Registration.SourceApp, null, Now);

        reg.MarkAttended(Now);

        reg.Status.Should().Be(RegistrationStatus.Attended);
        reg.AttendedAt.Should().Be(Now);
    }

    [Fact]
    public void Post_Hide_moves_status_to_hidden()
    {
        var post = Post.Create(Guid.NewGuid(), "img", "légende", null, Now);
        post.Status.Should().Be(Post.StatusPublished);

        post.Hide(Now);

        post.Status.Should().Be(Post.StatusHidden);
    }
}
