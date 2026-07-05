using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Infrastructure.Identity;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Dev;

/// <summary>Compte-rendu d'un seed de développement.</summary>
public sealed record SeedResult(
    int Users, int Categories, int Organizers, int Activities,
    int Registrations, int Hearts, int Posts, int Comments, int Likes, int Reports);

/// <summary>
/// Seeder de données de DÉVELOPPEMENT. Génère un jeu réaliste (faux users +
/// activités + inscriptions + cœurs + social) pour peupler le back-office et
/// l'app. Les données de seed sont TAGUÉES (users/organisateurs en
/// <c>@seed.local</c>, catégories en slug <c>seed-*</c>) et un reseed commence
/// par les effacer — sans jamais toucher aux vrais utilisateurs du tenant
/// (ceux-là ont un <c>EntraObjectId</c> et un courriel réel).
///
/// Les faux users ont <c>EntraObjectId = null</c> : ils n'ont aucune identité
/// Microsoft et ne peuvent donc jamais se connecter — ils n'existent que comme
/// données. Seuls les vrais membres du tenant s'authentifient.
/// </summary>
public sealed class DevDataSeeder
{
    private const string SeedEmailDomain = "@seed.local";
    private const string SeedSlugPrefix = "seed-";

    private readonly EventHubDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DevDataSeeder(EventHubDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<SeedResult> ResetAndSeedAsync(CancellationToken ct = default)
    {
        await ResetAsync(ct);
        return await SeedAsync(ct);
    }

    // ---------------------------------------------------------------- Reset ---

    private async Task ResetAsync(CancellationToken ct)
    {
        var seedUserIds = await _db.Users
            .Where(u => u.Email != null && u.Email.EndsWith(SeedEmailDomain))
            .Select(u => u.Id).ToListAsync(ct);

        var seedCategoryIds = await _db.Categories
            .Where(c => c.Slug.StartsWith(SeedSlugPrefix))
            .Select(c => c.Id).ToListAsync(ct);

        var seedOrganizerIds = await _db.Organizers
            .Where(o => o.ContactEmail != null && o.ContactEmail.EndsWith(SeedEmailDomain))
            .Select(o => o.Id).ToListAsync(ct);

        var seedActivityIds = await _db.Activities
            .Where(a => seedCategoryIds.Contains(a.CategoryId)
                        || (a.OrganizerId != null && seedOrganizerIds.Contains(a.OrganizerId.Value)))
            .Select(a => a.Id).ToListAsync(ct);

        var seedPostIds = await _db.Posts
            .Where(p => seedUserIds.Contains(p.AuthorId))
            .Select(p => p.Id).ToListAsync(ct);

        // Suppression dans l'ordre des dépendances (enfants d'abord).
        await _db.Reports
            .Where(r => seedUserIds.Contains(r.ReporterId) || seedPostIds.Contains(r.TargetId))
            .ExecuteDeleteAsync(ct);
        await _db.Comments
            .Where(c => seedUserIds.Contains(c.AuthorId) || seedPostIds.Contains(c.PostId))
            .ExecuteDeleteAsync(ct);
        await _db.PostLikes
            .Where(l => seedUserIds.Contains(l.UserId) || seedPostIds.Contains(l.PostId))
            .ExecuteDeleteAsync(ct);
        await _db.Posts.Where(p => seedUserIds.Contains(p.AuthorId)).ExecuteDeleteAsync(ct);
        await _db.HeartTransactions
            .Where(h => seedUserIds.Contains(h.UserId)
                        || (h.ActivityId != null && seedActivityIds.Contains(h.ActivityId.Value)))
            .ExecuteDeleteAsync(ct);
        await _db.Registrations
            .Where(r => seedUserIds.Contains(r.UserId) || seedActivityIds.Contains(r.ActivityId))
            .ExecuteDeleteAsync(ct);
        await _db.Notifications.Where(n => seedUserIds.Contains(n.UserId)).ExecuteDeleteAsync(ct);
        await _db.Devices.Where(d => seedUserIds.Contains(d.UserId)).ExecuteDeleteAsync(ct);
        await _db.NotificationSettings.Where(s => seedUserIds.Contains(s.UserId)).ExecuteDeleteAsync(ct);
        await _db.Activities.Where(a => seedActivityIds.Contains(a.Id)).ExecuteDeleteAsync(ct);
        await _db.Organizers.Where(o => seedOrganizerIds.Contains(o.Id)).ExecuteDeleteAsync(ct);
        await _db.Categories.Where(c => seedCategoryIds.Contains(c.Id)).ExecuteDeleteAsync(ct);
        // Les faux users en dernier (cascade DB vers AspNetUserRoles).
        await _db.Users.Where(u => seedUserIds.Contains(u.Id)).ExecuteDeleteAsync(ct);
    }

    // ----------------------------------------------------------------- Seed ---

    private static readonly string[] FirstNames =
        { "Émma", "Léo", "Chloé", "Nathan", "Jade", "Lucas", "Alice", "Hugo", "Léa", "Louis",
          "Manon", "Gabriel", "Camille", "Raphaël", "Sarah", "Adam", "Zoé", "Noah", "Inès", "Tom" };

    private static readonly string[] LastNames =
        { "Tremblay", "Gagnon", "Roy", "Côté", "Bouchard", "Gauthier", "Morin", "Lavoie",
          "Fortin", "Gagné", "Ouellet", "Pelletier", "Bélanger", "Lévesque", "Bergeron" };

    private async Task<SeedResult> SeedAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rnd = new Random(20260705); // déterministe

        // 1) Catégories
        var categories = new[]
        {
            Category.Create("seed-sport", "Sport & plein air", "#107c10", "sport"),
            Category.Create("seed-culture", "Socioculturel", "#8764b8", "culture"),
            Category.Create("seed-sante", "Santé & bien-être", "#00b7c3", "sante"),
            Category.Create("seed-academique", "Académique", "#0078d4", "academique"),
            Category.Create("seed-benevolat", "Bénévolat", "#d83b01", "benevolat"),
        };
        await _db.Categories.AddRangeAsync(categories, ct);

        // 2) Organisateurs
        var organizers = new[]
        {
            Organizer.Create("[seed] AGE UQTR", "age@seed.local"),
            Organizer.Create("[seed] Service des sports", "sports@seed.local"),
            Organizer.Create("[seed] Vie étudiante", "vie@seed.local"),
            Organizer.Create("[seed] Bureau de la santé", "sante@seed.local"),
        };
        await _db.Organizers.AddRangeAsync(organizers, ct);
        await _db.SaveChangesAsync(ct); // catégories + organisateurs => Ids

        // 3) Faux utilisateurs (jamais de login : EntraObjectId null, sans mdp)
        var users = new List<ApplicationUser>();
        for (var i = 0; i < 40; i++)
        {
            var first = FirstNames[rnd.Next(FirstNames.Length)];
            var last = LastNames[rnd.Next(LastNames.Length)];
            var email = $"user{i:D2}{SeedEmailDomain}";
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                Name = $"{first} {last}",
                Status = "active",
                EntraObjectId = null,
                EmailConfirmed = true,
            };
            var created = await _users.CreateAsync(user);
            if (!created.Succeeded) continue;
            await _users.AddToRoleAsync(user, i % 12 == 0 ? "organizer" : "student");
            users.Add(user);
        }

        // 4) Activités (statuts, dates passées/futures, à la une)
        var statuses = new[] { ActivityStatus.Published, ActivityStatus.Published,
            ActivityStatus.Published, ActivityStatus.Draft, ActivityStatus.Cancelled,
            ActivityStatus.Archived };
        var titles = new[]
        {
            "Yoga matinal", "Tournoi de basketball", "Atelier de poterie", "Course en forêt",
            "Conférence : santé mentale", "Cinéma en plein air", "Atelier de peinture",
            "Méditation guidée", "Festival culturel", "Tournoi de volleyball", "5 à 7 réseautage",
            "Randonnée d'automne", "Clinique de CV", "Soirée jeux de société", "Don de sang",
            "Atelier cuisine", "Match d'impro", "Marche solidaire",
        };

        var activities = new List<Activity>();
        for (var i = 0; i < titles.Length; i++)
        {
            var status = statuses[i % statuses.Length];
            var isPast = i % 3 == 0;
            var startsAt = isPast ? now.AddDays(-rnd.Next(3, 40)) : now.AddDays(rnd.Next(2, 45));
            var category = categories[i % categories.Length];
            var organizer = organizers[i % organizers.Length];
            var activity = Activity.Create(
                title: titles[i],
                description: $"Activité de démonstration « {titles[i]} » générée pour le développement.",
                categoryId: category.Id,
                organizerId: organizer.Id,
                startsAt: startsAt,
                endsAt: startsAt.AddHours(2),
                location: "Pavillon Ringuet, UQTR",
                imageUrl: $"https://picsum.photos/seed/act{i}/800/450",
                heartsReward: new[] { 10, 20, 25, 30, 40, 50 }[rnd.Next(6)],
                maxParticipants: new[] { 10, 15, 20, 25, 30 }[rnd.Next(5)],
                registrationUrl: i % 4 == 0 ? "https://forms.gle/exemple" : null,
                registrationDeadline: startsAt.AddDays(-1),
                isFeatured: i % 5 == 0,
                status: status,
                nowUtc: now);
            activities.Add(activity);
        }
        await _db.Activities.AddRangeAsync(activities, ct);
        await _db.SaveChangesAsync(ct); // activités => Ids

        // 5) Inscriptions + crédits de cœurs
        var registrations = 0;
        var heartsCount = 0;
        foreach (var activity in activities.Where(a => a.Status == ActivityStatus.Published))
        {
            var isPast = activity.StartsAt < now;
            var shuffled = users.OrderBy(_ => rnd.Next()).ToList();
            var take = Math.Min(shuffled.Count, activity.MaxParticipants + rnd.Next(0, 5));
            for (var idx = 0; idx < take; idx++)
            {
                var user = shuffled[idx];
                RegistrationStatus status;
                if (idx >= activity.MaxParticipants)
                    status = RegistrationStatus.Waitlisted;
                else if (isPast)
                    status = rnd.Next(100) < 75 ? RegistrationStatus.Attended : RegistrationStatus.NoShow;
                else
                    status = RegistrationStatus.Registered;

                _db.Registrations.Add(Registration.Create(
                    user.Id, activity.Id, status, "seed", null, now));
                registrations++;

                if (status == RegistrationStatus.Attended && activity.HeartsReward > 0)
                {
                    _db.HeartTransactions.Add(HeartTransaction.ForAttendance(
                        user.Id, activity.Id, activity.Title, activity.HeartsReward, now));
                    heartsCount++;
                }
            }
        }

        // 6) Social : posts + commentaires
        var posts = new List<Post>();
        for (var i = 0; i < 10; i++)
        {
            var author = users[rnd.Next(users.Count)];
            var linked = activities[rnd.Next(activities.Count)];
            var post = Post.Create(
                author.Id,
                $"https://picsum.photos/seed/post{i}/600/600",
                $"Superbe moment à « {linked.Title} » ! 🎉",
                i % 2 == 0 ? linked.Id : null,
                now.AddDays(-rnd.Next(0, 20)));
            posts.Add(post);
        }
        await _db.Posts.AddRangeAsync(posts, ct);

        var comments = new List<Comment>();
        var likesCount = 0;
        var commentTexts = new[]
        {
            "Bravo, ça donne envie ! 👏", "J'étais là, super moment !",
            "Vraiment nul comme activité...", "Trop cher pour ce que c'est.",
            "On se voit là-bas 😄", "N'importe quoi cette organisation.",
        };
        foreach (var post in posts)
        {
            var commentCount = rnd.Next(1, 4);
            for (var c = 0; c < commentCount; c++)
            {
                var author = users[rnd.Next(users.Count)];
                var comment = Comment.Create(
                    post.Id, author.Id, commentTexts[rnd.Next(commentTexts.Length)], now);
                _db.Comments.Add(comment);
                comments.Add(comment);
            }

            // Likes : un sous-ensemble distinct d'utilisateurs par publication.
            foreach (var liker in users.OrderBy(_ => rnd.Next()).Take(rnd.Next(0, 15)))
            {
                _db.PostLikes.Add(PostLike.Create(post.Id, liker.Id, now));
                likesCount++;
            }
        }

        // 7) Modération : signalements ouverts — publications ET commentaires
        var reports = 0;
        var reportReasons = new[]
        {
            "Contenu inapproprié (démo).", "Propos déplacés (démo).",
            "Spam / hors-sujet (démo).", "Harcèlement présumé (démo).",
        };
        foreach (var post in posts.Take(2))
        {
            var reporter = users[rnd.Next(users.Count)];
            _db.Reports.Add(Report.Create(
                reporter.Id, ReportTargetType.Post, post.Id,
                reportReasons[rnd.Next(reportReasons.Length)], now));
            reports++;
        }
        foreach (var comment in comments.OrderBy(_ => rnd.Next()).Take(3))
        {
            var reporter = users[rnd.Next(users.Count)];
            _db.Reports.Add(Report.Create(
                reporter.Id, ReportTargetType.Comment, comment.Id,
                reportReasons[rnd.Next(reportReasons.Length)], now));
            reports++;
        }

        await _db.SaveChangesAsync(ct);

        return new SeedResult(
            users.Count, categories.Length, organizers.Length, activities.Count,
            registrations, heartsCount, posts.Count, comments.Count, likesCount, reports);
    }
}
