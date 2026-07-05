namespace EventHub.Domain.Services;

/// <summary>
/// Port d'envoi de notifications push (FCM/APNs). L'implémentation réelle vit
/// dans l'Infrastructure ; un adaptateur no-op journalise tant que la config
/// du fournisseur push n'est pas fournie.
/// </summary>
public interface IPushSender
{
    /// <summary>Envoie un push à tous les appareils enregistrés de l'utilisateur.</summary>
    Task SendAsync(
        Guid userId, string title, string body, CancellationToken cancellationToken = default);
}
