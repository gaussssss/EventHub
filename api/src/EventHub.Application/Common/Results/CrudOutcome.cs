namespace EventHub.Application.Common.Results;

/// <summary>Issue générique d'une écriture CRUD sur un référentiel.</summary>
public enum CrudOutcome
{
    Done,
    NotFound,
    Conflict,
}
