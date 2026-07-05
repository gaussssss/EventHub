using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Uploads;

/// <summary>Obtenir un ticket d'upload pré-signé (POST /api/uploads/sign).</summary>
public sealed record SignUploadQuery(string Type, string ContentType) : IQuery<UploadTicket>;
