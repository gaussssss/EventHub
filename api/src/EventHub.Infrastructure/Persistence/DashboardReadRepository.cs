using EventHub.Domain.Enums;
using EventHub.Domain.Repositories;
using EventHub.Domain.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

/// <summary>Agrégats du tableau de bord et export CSV (calculés à la volée).</summary>
public sealed class DashboardReadRepository : IDashboardReadRepository
{
    private readonly EventHubDbContext _db;

    public DashboardReadRepository(EventHubDbContext db) => _db = db;

    public async Task<DashboardOverviewDto> GetOverviewAsync(
        DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        return new DashboardOverviewDto
        {
            TotalUsers = await _db.Users.CountAsync(cancellationToken),
            TotalActivities = await _db.Activities.CountAsync(cancellationToken),
            PublishedActivities = await _db.Activities
                .CountAsync(a => a.Status == ActivityStatus.Published, cancellationToken),
            UpcomingActivities = await _db.Activities
                .CountAsync(a => a.Status == ActivityStatus.Published && a.StartsAt > nowUtc,
                    cancellationToken),
            TotalRegistrations = await _db.Registrations
                .CountAsync(r => r.Status == RegistrationStatus.Registered ||
                                 r.Status == RegistrationStatus.Attended, cancellationToken),
            WaitlistedRegistrations = await _db.Registrations
                .CountAsync(r => r.Status == RegistrationStatus.Waitlisted, cancellationToken),
            TotalHeartsAwarded = await _db.HeartTransactions
                .SumAsync(h => (long)h.Hearts, cancellationToken),
            TotalPosts = await _db.Posts.CountAsync(cancellationToken),
        };
    }

    public async Task<ActivityDashboardDto?> GetActivityDashboardAsync(
        Guid activityId, CancellationToken cancellationToken = default)
    {
        var activity = await _db.Activities.AsNoTracking()
            .Where(a => a.Id == activityId)
            .Select(a => new { a.Id, a.Title, a.MaxParticipants })
            .FirstOrDefaultAsync(cancellationToken);
        if (activity is null)
            return null;

        var regs = _db.Registrations.Where(r => r.ActivityId == activityId);
        var registered = await regs.CountAsync(r => r.Status == RegistrationStatus.Registered, cancellationToken);
        var attended = await regs.CountAsync(r => r.Status == RegistrationStatus.Attended, cancellationToken);
        var waitlisted = await regs.CountAsync(r => r.Status == RegistrationStatus.Waitlisted, cancellationToken);
        var noShow = await regs.CountAsync(r => r.Status == RegistrationStatus.NoShow, cancellationToken);
        var cancelled = await regs.CountAsync(r => r.Status == RegistrationStatus.Cancelled, cancellationToken);

        var active = registered + attended;      // occupent une place
        var withOutcome = attended + noShow;      // ont une issue de présence

        return new ActivityDashboardDto
        {
            ActivityId = activity.Id,
            Title = activity.Title,
            MaxParticipants = activity.MaxParticipants,
            Registered = registered,
            Attended = attended,
            Waitlisted = waitlisted,
            NoShow = noShow,
            Cancelled = cancelled,
            FillRate = activity.MaxParticipants > 0
                ? Math.Round((double)active / activity.MaxParticipants, 2)
                : 0,
            AttendanceRate = active > 0 ? Math.Round((double)attended / active, 2) : 0,
            NoShowRate = withOutcome > 0 ? Math.Round((double)noShow / withOutcome, 2) : 0,
        };
    }

    public async Task<IReadOnlyList<RegistrationExportRow>> GetRegistrationsForExportAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _db.Registrations
            .AsNoTracking()
            .Join(_db.Activities, r => r.ActivityId, a => a.Id, (r, a) => new { r, a })
            .Join(_db.Users, x => x.r.UserId, u => u.Id, (x, u) => new
            {
                x.r.ActivityId,
                ActivityTitle = x.a.Title,
                x.r.UserId,
                UserName = u.Name,
                UserEmail = u.Email,
                x.r.Status,
                x.r.RegisteredAt,
            })
            .OrderBy(x => x.ActivityTitle)
            .ThenBy(x => x.RegisteredAt)
            .ToListAsync(cancellationToken);

        return rows
            .Select(x => new RegistrationExportRow
            {
                ActivityId = x.ActivityId,
                ActivityTitle = x.ActivityTitle,
                UserId = x.UserId,
                UserName = x.UserName,
                UserEmail = x.UserEmail,
                Status = x.Status.ToString().ToLowerInvariant(),
                RegisteredAt = x.RegisteredAt,
            })
            .ToList();
    }
}
