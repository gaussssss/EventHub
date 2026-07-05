using EventHub.Domain.ReadModels;
using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Admin;
using EventHub.Application.Registrations;
using EventHub.Application.Hearts;
using EventHub.Application.Profile;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class CreateActivityHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IActivityRepository> _activities = new();
    private readonly Mock<ICategoryRepository> _categories = new();

    private CreateActivityHandler Handler() =>
        new(_activities.Object, _categories.Object, Mock.Of<IClock>(c => c.UtcNow == Now));

    private static CreateActivityCommand Command(Guid categoryId) => new(
        "Course", "…", categoryId, null, Now.AddDays(10), null, "Parc", "https://img",
        30, 40, null, null, false, ActivityStatus.Published);

    [Fact]
    public async Task Creates_when_category_exists()
    {
        var categoryId = Guid.NewGuid();
        _categories.Setup(c => c.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await Handler().HandleAsync(Command(categoryId));

        result.Status.Should().Be(CreateActivityStatus.Created);
        result.ActivityId.Should().NotBeNull();
        _activities.Verify(a => a.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Once);
        _activities.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Fails_when_category_missing()
    {
        var categoryId = Guid.NewGuid();
        _categories.Setup(c => c.ExistsAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await Handler().HandleAsync(Command(categoryId));

        result.Status.Should().Be(CreateActivityStatus.CategoryNotFound);
        _activities.Verify(a => a.AddAsync(It.IsAny<Activity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

public class UpdateActivityHandlerTests
{
    private static readonly DateTime Now = new(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _id = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Mock<IActivityRepository> _activities = new();
    private readonly Mock<ICategoryRepository> _categories = new();

    private UpdateActivityHandler Handler() =>
        new(_activities.Object, _categories.Object, Mock.Of<IClock>(c => c.UtcNow == Now));

    private UpdateActivityCommand Command() => new(
        _id, "Nouveau titre", "…", _categoryId, null, Now.AddDays(10), null, "Parc",
        "https://img", 30, 40, null, null, true, ActivityStatus.Cancelled);

    [Fact]
    public async Task Updates_existing_activity()
    {
        var activity = Activity.Create(
            "Ancien", "…", Guid.NewGuid(), null, Now, null, "x", "y",
            0, 10, null, null, isFeatured: false, ActivityStatus.Published, Now);
        _activities.Setup(a => a.GetAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _categories.Setup(c => c.ExistsAsync(_categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var status = await Handler().HandleAsync(Command());

        status.Should().Be(UpdateActivityStatus.Updated);
        activity.Title.Should().Be("Nouveau titre");
        activity.Status.Should().Be(ActivityStatus.Cancelled);
        activity.IsFeatured.Should().BeTrue();
        _activities.Verify(a => a.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Returns_not_found_when_activity_missing()
    {
        _activities.Setup(a => a.GetAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync((Activity?)null);

        var status = await Handler().HandleAsync(Command());

        status.Should().Be(UpdateActivityStatus.NotFound);
    }
}

public class GetProfileHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Mock<IUserReadRepository> _users = new();
    private readonly Mock<IHeartReadRepository> _hearts = new();
    private readonly Mock<IRegistrationRepository> _registrations = new();

    private GetProfileHandler Handler() => new(_users.Object, _hearts.Object, _registrations.Object);

    [Fact]
    public async Task Composes_profile_with_level_and_registration_count()
    {
        _users.Setup(u => u.GetAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserInfo(_userId, "Alex", "alex@uqtr.ca", "https://a"));
        _hearts.Setup(h => h.GetTotalAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(340);
        _registrations.Setup(r => r.GetActiveActivityIdsAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { Guid.NewGuid(), Guid.NewGuid() });

        var profile = await Handler().HandleAsync(new GetProfileQuery(_userId));

        profile.Should().NotBeNull();
        profile!.Name.Should().Be("Alex");
        profile.TotalHearts.Should().Be(340);
        profile.Level.Should().Be("Argent");
        profile.RegistrationCount.Should().Be(2);
    }

    [Fact]
    public async Task Returns_null_when_user_unknown()
    {
        _users.Setup(u => u.GetAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInfo?)null);

        (await Handler().HandleAsync(new GetProfileQuery(_userId))).Should().BeNull();
    }
}
