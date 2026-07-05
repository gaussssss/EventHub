using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using EventHub.Application.Hearts;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventHub.UnitTests.Application;

public class GetHeartsSummaryHandlerTests
{
    [Fact]
    public async Task Composes_total_level_and_history()
    {
        var userId = Guid.NewGuid();
        var history = new List<HeartHistoryDto>
        {
            new("Yoga matinal", 20, new DateTime(2026, 5, 15, 0, 0, 0, DateTimeKind.Utc)),
        };

        var repo = new Mock<IHeartReadRepository>();
        repo.Setup(r => r.GetTotalAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(340);
        repo.Setup(r => r.GetHistoryAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await new GetHeartsSummaryHandler(repo.Object)
            .HandleAsync(new GetHeartsSummaryQuery(userId));

        result.TotalHearts.Should().Be(340);
        result.Level.Should().Be("Argent");        // 200 <= 340 < 500
        result.PreviousThreshold.Should().Be(200);
        result.NextThreshold.Should().Be(500);
        result.History.Should().ContainSingle(h => h.ActivityTitle == "Yoga matinal");
    }
}
