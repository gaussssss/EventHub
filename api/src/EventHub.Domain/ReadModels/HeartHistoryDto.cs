namespace EventHub.Domain.ReadModels;

/// <summary>Une entrée d'historique de cœurs (heartHistory de l'app mobile).</summary>
public sealed record HeartHistoryDto(string ActivityTitle, int Hearts, DateTime Date);
