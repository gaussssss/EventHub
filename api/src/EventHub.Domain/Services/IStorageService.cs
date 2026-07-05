namespace EventHub.Domain.Services;

/// <summary>Ticket d'upload pré-signé : URL d'envoi + URL publique finale.</summary>
public sealed record UploadTicket(string UploadUrl, string FileUrl);

/// <summary>
/// Port de stockage objet (S3-compatible). L'implémentation réelle génère des
/// URL pré-signées ; un adaptateur de substitution fabrique des URL
/// déterministes tant que le stockage n'est pas provisionné.
/// </summary>
public interface IStorageService
{
    /// <summary>Prépare un emplacement d'upload pour un type de média donné.</summary>
    UploadTicket CreateUploadTicket(string type, string contentType);
}
