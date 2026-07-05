using EventHub.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace EventHub.UnitTests.Domain;

public class HeartLevelTests
{
    [Theory]
    [InlineData(0, "Bronze", 0, 200)]
    [InlineData(199, "Bronze", 0, 200)]
    [InlineData(200, "Argent", 200, 500)]
    [InlineData(340, "Argent", 200, 500)]
    [InlineData(499, "Argent", 200, 500)]
    [InlineData(500, "Or", 500, 1000)]
    [InlineData(1200, "Or", 500, 1000)]
    public void FromHearts_maps_total_to_level_and_thresholds(
        int total, string name, int previous, int next)
    {
        var level = HeartLevel.FromHearts(total);

        level.Name.Should().Be(name);
        level.PreviousThreshold.Should().Be(previous);
        level.NextThreshold.Should().Be(next);
    }
}
