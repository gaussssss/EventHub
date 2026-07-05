using EventHub.Domain.Services;
using EventHub.Application.Common.Messaging;

namespace EventHub.Application.Uploads;

public sealed class SignUploadHandler : IQueryHandler<SignUploadQuery, UploadTicket>
{
    private readonly IStorageService _storage;

    public SignUploadHandler(IStorageService storage) => _storage = storage;

    public Task<UploadTicket> HandleAsync(
        SignUploadQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(_storage.CreateUploadTicket(query.Type, query.ContentType));
}
