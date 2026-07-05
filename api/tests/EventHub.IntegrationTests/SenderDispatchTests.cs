using EventHub.Application.Common.Messaging;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventHub.IntegrationTests;

/// <summary>
/// Vérifie le médiateur maison : <see cref="Sender"/> résout le handler
/// correspondant au type concret du message et lui délègue l'exécution.
/// </summary>
public class SenderDispatchTests
{
    // Types publics : le dispatch dynamique du Sender (assembly Infrastructure)
    // doit pouvoir lier la méthode HandleAsync du handler concret.
    public sealed record Ping(string Text) : ICommand<string>;
    public sealed record Count(int N) : IQuery<int>;

    public sealed class PingHandler : ICommandHandler<Ping, string>
    {
        public Task<string> HandleAsync(Ping command, CancellationToken ct = default)
            => Task.FromResult($"pong:{command.Text}");
    }

    public sealed class CountHandler : IQueryHandler<Count, int>
    {
        public Task<int> HandleAsync(Count query, CancellationToken ct = default)
            => Task.FromResult(query.N * 2);
    }

    private static ISender BuildSender()
    {
        var services = new ServiceCollection();
        services.AddScoped<ICommandHandler<Ping, string>, PingHandler>();
        services.AddScoped<IQueryHandler<Count, int>, CountHandler>();
        services.AddScoped<ISender, Sender>();
        return services.BuildServiceProvider().GetRequiredService<ISender>();
    }

    [Fact]
    public async Task Routes_command_to_its_handler()
    {
        var result = await BuildSender().Send(new Ping("hi"));
        result.Should().Be("pong:hi");
    }

    [Fact]
    public async Task Routes_query_to_its_handler()
    {
        var result = await BuildSender().Send(new Count(21));
        result.Should().Be(42);
    }

    [Fact]
    public async Task Throws_when_no_handler_registered()
    {
        var services = new ServiceCollection();
        services.AddScoped<ISender, Sender>();
        var sender = services.BuildServiceProvider().GetRequiredService<ISender>();

        var act = () => sender.Send(new Ping("x"));
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
