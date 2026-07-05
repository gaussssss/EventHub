using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Application.Common.Messaging;

/// <summary>
/// Résout le handler correspondant au type concret du message et lui délègue
/// l'exécution. Le dispatch dynamique évite tout registre manuel message→handler.
/// </summary>
public sealed class Sender : ISender
{
    private readonly IServiceProvider _provider;

    public Sender(IServiceProvider provider) => _provider = provider;

    public Task<TResult> Send<TResult>(
        ICommand<TResult> command, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(ICommandHandler<,>)
            .MakeGenericType(command.GetType(), typeof(TResult));
        dynamic handler = _provider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public Task<TResult> Send<TResult>(
        IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IQueryHandler<,>)
            .MakeGenericType(query.GetType(), typeof(TResult));
        dynamic handler = _provider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, cancellationToken);
    }
}
