namespace EventHub.Domain.Services;

/// <summary>Horloge injectable (rend le temps testable).</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
