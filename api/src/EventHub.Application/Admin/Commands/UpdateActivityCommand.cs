using EventHub.Application.Common.Messaging;
using EventHub.Domain.Enums;

namespace EventHub.Application.Admin;

public sealed record UpdateActivityCommand(
    Guid Id,
    string Title,
    string Description,
    Guid CategoryId,
    Guid? OrganizerId,
    DateTime StartsAt,
    DateTime? EndsAt,
    string Location,
    string ImageUrl,
    int HeartsReward,
    int MaxParticipants,
    string? RegistrationUrl,
    DateTime? RegistrationDeadline,
    bool IsFeatured,
    ActivityStatus Status) : ICommand<UpdateActivityStatus>;
