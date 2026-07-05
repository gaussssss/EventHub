using System.Net.Http.Json;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Identity;
using FluentAssertions;
using Xunit;

namespace EventHub.IntegrationTests;

public class LeaderboardEndpointsTests : IClassFixture<EventHubApiFactory>
{
    private readonly EventHubApiFactory _factory;

    public LeaderboardEndpointsTests(EventHubApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GET_leaderboard_ranks_users_by_hearts_and_flags_me()
    {
        var me = Guid.NewGuid();
        var top = Guid.NewGuid();
        await _factory.WithDbAsync(async db =>
        {
            db.Users.Add(new ApplicationUser { Id = me, UserName = $"u-{me:N}", Name = "Moi" });
            db.Users.Add(new ApplicationUser { Id = top, UserName = $"u-{top:N}", Name = "Champion" });
            db.HeartTransactions.Add(HeartTransaction.ForAttendance(me, Guid.NewGuid(), "A", 100, DateTime.UtcNow));
            db.HeartTransactions.Add(HeartTransaction.ForAttendance(top, Guid.NewGuid(), "B", 300, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User-Id", me.ToString());

        var board = (await client.GetFromJsonAsync<List<Row>>("/api/leaderboard"))!;

        board.Should().HaveCountGreaterThanOrEqualTo(2);
        board[0].Name.Should().Be("Champion");
        board[0].Rank.Should().Be(1);
        board.Single(r => r.Name == "Moi").IsMe.Should().BeTrue();
        board.Single(r => r.Name == "Champion").IsMe.Should().BeFalse();
    }

    [Fact]
    public async Task GET_stats_community_returns_totals()
    {
        await _factory.WithDbAsync(async db =>
        {
            var u = Guid.NewGuid();
            db.Users.Add(new ApplicationUser { Id = u, UserName = $"u-{u:N}", Name = "S" });
            db.HeartTransactions.Add(HeartTransaction.ForAttendance(u, Guid.NewGuid(), "C", 42, DateTime.UtcNow));
            await db.SaveChangesAsync();
        });

        var stats = await _factory.CreateClient()
            .GetFromJsonAsync<StatsRow>("/api/stats/community");

        stats!.TotalRegisteredUsers.Should().BeGreaterThan(0);
        stats.TotalUqtrHearts.Should().BeGreaterThanOrEqualTo(42);
    }

    private sealed record Row
    {
        public int Rank { get; init; }
        public string? Name { get; init; }
        public bool IsMe { get; init; }
    }

    private sealed record StatsRow
    {
        public int TotalRegisteredUsers { get; init; }
        public long TotalUqtrHearts { get; init; }
    }
}
