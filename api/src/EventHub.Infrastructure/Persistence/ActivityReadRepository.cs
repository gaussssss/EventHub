using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using System.Linq.Expressions;
using EventHub.Application.Activities;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>
/// Projection EF Core des activités publiées vers <see cref="ActivityDto"/>,
/// avec calcul du nombre d'inscrits (statuts Registered/Attended).
/// </summary>
public sealed class ActivityReadRepository : IActivityReadRepository
{
    private readonly EventHubDbContext _db;

    public ActivityReadRepository(EventHubDbContext db) => _db = db;

    public async Task<IReadOnlyList<ActivityDto>> GetActivitiesAsync(
        ActivityFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Activities.AsNoTracking()
            .Where(a => a.Status == ActivityStatus.Published);

        if (filter.FeaturedOnly)
            query = query.Where(a => a.IsFeatured);

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(a => a.Category!.Slug == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim();
            query = query.Where(a =>
                EF.Functions.Like(a.Title, $"%{term}%") ||
                EF.Functions.Like(a.Location, $"%{term}%"));
        }

        if (filter.From is not null)
            query = query.Where(a => a.StartsAt >= filter.From.Value);

        if (filter.To is not null)
            query = query.Where(a => a.StartsAt <= filter.To.Value);

        if (filter.AvailableOnly)
            query = query.Where(a => _db.Registrations.Count(r =>
                r.ActivityId == a.Id &&
                (r.Status == RegistrationStatus.Registered ||
                 r.Status == RegistrationStatus.Attended)) < a.MaxParticipants);

        query = filter.Descending
            ? query.OrderByDescending(a => a.StartsAt)
            : query.OrderBy(a => a.StartsAt);

        return await query
            .Select(ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<ActivityDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _db.Activities.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(ToDto())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdminActivityDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken = default)
    {
        // Projection en type anonyme (traduisible SQL) puis mapping en mémoire :
        // le statut (enum) est converti en chaîne côté client pour éviter les
        // écueils de traduction de ToString() sur enum selon le fournisseur.
        var rows = await _db.Activities.AsNoTracking()
            .OrderByDescending(a => a.StartsAt)
            .Select(a => new
            {
                a.Id,
                a.Title,
                Category = a.Category!.Slug,
                a.StartsAt,
                a.Location,
                a.Status,
                a.IsFeatured,
                a.MaxParticipants,
                Current = _db.Registrations.Count(r =>
                    r.ActivityId == a.Id &&
                    (r.Status == RegistrationStatus.Registered ||
                     r.Status == RegistrationStatus.Attended)),
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new AdminActivityDto
        {
            Id = r.Id,
            Title = r.Title,
            Category = r.Category,
            StartsAt = r.StartsAt,
            Location = r.Location,
            Status = r.Status.ToString().ToLowerInvariant(),
            IsFeatured = r.IsFeatured,
            MaxParticipants = r.MaxParticipants,
            CurrentParticipants = r.Current,
        }).ToList();
    }

    public async Task<AdminActivityDetailDto?> GetForAdminByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _db.Activities.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new
            {
                a.Id,
                a.Title,
                a.Description,
                a.CategoryId,
                a.OrganizerId,
                a.StartsAt,
                a.EndsAt,
                a.Location,
                a.ImageUrl,
                a.HeartsReward,
                a.MaxParticipants,
                a.ParticipationCost,
                a.RegistrationUrl,
                a.RegistrationDeadline,
                a.IsFeatured,
                a.Status,
                a.CheckInToken,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (r is null) return null;

        return new AdminActivityDetailDto
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            CategoryId = r.CategoryId,
            OrganizerId = r.OrganizerId,
            StartsAt = r.StartsAt,
            EndsAt = r.EndsAt,
            Location = r.Location,
            ImageUrl = r.ImageUrl,
            HeartsReward = r.HeartsReward,
            MaxParticipants = r.MaxParticipants,
            ParticipationCost = r.ParticipationCost,
            RegistrationUrl = r.RegistrationUrl,
            RegistrationDeadline = r.RegistrationDeadline,
            IsFeatured = r.IsFeatured,
            Status = r.Status.ToString().ToLowerInvariant(),
            CheckInToken = r.CheckInToken,
        };
    }

    public async Task<IReadOnlyList<ActivityDto>> GetRegisteredByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _db.Activities.AsNoTracking()
            .Where(a => _db.Registrations.Any(r =>
                r.ActivityId == a.Id &&
                r.UserId == userId &&
                r.Status != RegistrationStatus.Cancelled))
            .OrderBy(a => a.StartsAt)
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Category = a.Category!.Slug,
                Organizer = a.Organizer != null ? a.Organizer.Name : null,
                StartsAt = a.StartsAt,
                EndsAt = a.EndsAt,
                Location = a.Location,
                ImageUrl = a.ImageUrl,
                HeartsReward = a.HeartsReward,
                MaxParticipants = a.MaxParticipants,
                CurrentParticipants = _db.Registrations.Count(r =>
                    r.ActivityId == a.Id &&
                    (r.Status == RegistrationStatus.Registered ||
                     r.Status == RegistrationStatus.Attended)),
                ParticipationCost = a.ParticipationCost,
                RegistrationUrl = a.RegistrationUrl,
                RegistrationDeadline = a.RegistrationDeadline,
                IsFeatured = a.IsFeatured,
                // Statut de l'inscription courante (hors annulées) pour le calendrier.
                MyStatus = _db.Registrations
                    .Where(r => r.ActivityId == a.Id &&
                                r.UserId == userId &&
                                r.Status != RegistrationStatus.Cancelled)
                    .Select(r => r.Status.ToString().ToLower())
                    .FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Expression de projection (traduite en SQL). La sous-requête de comptage
    /// référence <c>_db.Registrations</c>, d'où une expression d'instance.
    /// </summary>
    private Expression<Func<Activity, ActivityDto>> ToDto() => a => new ActivityDto
    {
        Id = a.Id,
        Title = a.Title,
        Description = a.Description,
        Category = a.Category!.Slug,
        Organizer = a.Organizer != null ? a.Organizer.Name : null,
        StartsAt = a.StartsAt,
        EndsAt = a.EndsAt,
        Location = a.Location,
        ImageUrl = a.ImageUrl,
        HeartsReward = a.HeartsReward,
        MaxParticipants = a.MaxParticipants,
        CurrentParticipants = _db.Registrations.Count(r =>
            r.ActivityId == a.Id &&
            (r.Status == RegistrationStatus.Registered ||
             r.Status == RegistrationStatus.Attended)),
        ParticipationCost = a.ParticipationCost,
        RegistrationUrl = a.RegistrationUrl,
        RegistrationDeadline = a.RegistrationDeadline,
        IsFeatured = a.IsFeatured,
    };
}
