namespace EventHub.Application.Common.Messaging;

/// <summary>Message d'écriture (change l'état) produisant un résultat.</summary>
public interface ICommand<TResult>
{
}

/// <summary>Message de lecture (sans effet de bord) produisant un résultat.</summary>
public interface IQuery<TResult>
{
}

/// <summary>Traite une commande <typeparamref name="TCommand"/>.</summary>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>Traite une requête <typeparamref name="TQuery"/>.</summary>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Médiateur : achemine une commande/requête vers son handler unique (résolu par DI).
/// Seul point de couplage entre les contrôleurs et la couche Application.
/// </summary>
public interface ISender
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
    Task<TResult> Send<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
