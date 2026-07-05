using EventHub.Application.Common.Exceptions;
using EventHub.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Api.Middleware;

/// <summary>
/// Traduit les exceptions non gérées en réponses <c>ProblemDetails</c> (RFC 7807) :
/// invariants de domaine → 400, conflit de concurrence → 409, reste → 500.
/// Journalise le détail côté serveur sans le divulguer au client.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = Map(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Exception non gérée");
        else
            _logger.LogWarning(exception, "Requête rejetée : {Title}", title);

        httpContext.Response.StatusCode = status;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                // Le message métier est sûr à exposer ; pas les 500 internes.
                Detail = status < StatusCodes.Status500InternalServerError ? exception.Message : null,
            },
        });
    }

    private static (int Status, string Title) Map(Exception exception) => exception switch
    {
        DomainException => (StatusCodes.Status400BadRequest, "Requête invalide"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Requête invalide"),
        ConcurrencyConflictException =>
            (StatusCodes.Status409Conflict, "Conflit de concurrence, veuillez réessayer"),
        _ => (StatusCodes.Status500InternalServerError, "Erreur interne du serveur"),
    };
}
