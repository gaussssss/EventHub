using EventHub.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace EventHub.Infrastructure.Common;

/// <summary>
/// Stockage de substitution : fabrique des URL déterministes sous une base
/// configurable (<c>Storage:PublicBaseUrl</c>) sans provisionner S3. À remplacer
/// par un générateur d'URL pré-signées lorsque le stockage objet sera en place.
/// </summary>
public sealed class StubStorageService : IStorageService
{
    private readonly string _baseUrl;

    public StubStorageService(IConfiguration configuration) =>
        _baseUrl = (configuration["Storage:PublicBaseUrl"] ?? "https://storage.eventhub.uqtr.ca")
            .TrimEnd('/');

    public UploadTicket CreateUploadTicket(string type, string contentType)
    {
        var safeType = string.IsNullOrWhiteSpace(type) ? "misc" : type.Trim().ToLowerInvariant();
        var key = $"{safeType}/{Guid.NewGuid():N}";
        var fileUrl = $"{_baseUrl}/{key}";
        // En réel : URL pré-signée PUT. Ici, on renvoie une URL de substitution.
        var uploadUrl = $"{fileUrl}?upload=stub";
        return new UploadTicket(uploadUrl, fileUrl);
    }
}
