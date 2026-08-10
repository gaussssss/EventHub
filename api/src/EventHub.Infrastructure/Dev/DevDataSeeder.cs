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
/// l'app. Les données produites n'exhibent AUCUN marqueur visible (pas de
/// préfixe « [seed] », courriels réalistes) : le reseed les identifie par des
/// marqueurs invisibles — users à <c>EntraObjectId == null</c> (les vrais
/// membres du tenant en ont toujours un), catégories/organisateurs par listes
/// fixes connues du seeder — et efface aussi les données d'anciens seeds tagués.
///
/// Les faux users ont <c>EntraObjectId = null</c> : ils n'ont aucune identité
/// Microsoft et ne peuvent donc jamais se connecter — ils n'existent que comme
/// données. Seuls les vrais membres du tenant s'authentifient.
/// </summary>
public sealed class DevDataSeeder
{
    // Marqueurs d'anciens seeds (nettoyés au reset, plus jamais générés).
    private const string LegacyEmailDomain = "@seed.local";
    private const string LegacySlugPrefix = "seed-";

    /// <summary>Slugs des catégories gérées par le seeder (clé de reset).</summary>
    private static readonly string[] SeedCategorySlugs =
        { "sport", "socioculturel", "sante", "academique", "benevolat" };

    /// <summary>Noms des organisateurs gérés par le seeder (clé de reset).</summary>
    private static readonly string[] SeedOrganizerNames =
        { "AGE UQTR", "Service des sports", "Vie étudiante", "Bureau de la santé", "CIAS UQTR" };

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
        // Faux users = jamais d'identité Microsoft (EntraObjectId null). Les vrais
        // membres du tenant en reçoivent toujours un au provisioning JIT — ils ne
        // sont donc jamais touchés. Couvre aussi les anciens users @seed.local.
        var seedUserIds = await _db.Users
            .Where(u => u.EntraObjectId == null)
            .Select(u => u.Id).ToListAsync(ct);

        var seedCategoryIds = await _db.Categories
            .Where(c => c.Slug.StartsWith(LegacySlugPrefix)
                        || SeedCategorySlugs.Contains(c.Slug))
            .Select(c => c.Id).ToListAsync(ct);

        var seedOrganizerIds = await _db.Organizers
            .Where(o => (o.ContactEmail != null && o.ContactEmail.EndsWith(LegacyEmailDomain))
                        || o.Name.StartsWith("[seed]")
                        || SeedOrganizerNames.Contains(o.Name))
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

    /// <summary>« Émma » → « emma » : minuscules sans diacritiques (courriels).</summary>
    private static string Slugify(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var builder = new System.Text.StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }
        return builder.ToString().ToLowerInvariant();
    }

    private async Task<SeedResult> SeedAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var rnd = new Random(20260705); // déterministe

        // 1) Catégories (slugs propres, cf. SeedCategorySlugs pour le reset)
        var categories = new[]
        {
            Category.Create("sport", "Sport & plein air", "#107c10", "sport"),
            Category.Create("socioculturel", "Socioculturel", "#8764b8", "culture"),
            Category.Create("sante", "Santé & bien-être", "#00b7c3", "sante"),
            Category.Create("academique", "Académique", "#0078d4", "academique"),
            Category.Create("benevolat", "Bénévolat", "#d83b01", "benevolat"),
        };
        await _db.Categories.AddRangeAsync(categories, ct);

        // 2) Organisateurs (noms réels, cf. SeedOrganizerNames pour le reset)
        var organizers = new[]
        {
            Organizer.Create("AGE UQTR", "age@uqtr.ca"),
            Organizer.Create("Service des sports", "sports@uqtr.ca"),
            Organizer.Create("Vie étudiante", "vie.etudiante@uqtr.ca"),
            Organizer.Create("Bureau de la santé", "sante@uqtr.ca"),
            Organizer.Create("CIAS UQTR", "evenements.CIAS@uqtr.ca"),
        };
        await _db.Organizers.AddRangeAsync(organizers, ct);
        await _db.SaveChangesAsync(ct); // catégories + organisateurs => Ids

        // 3) Faux utilisateurs (jamais de login : EntraObjectId null, sans mdp).
        //    Courriels réalistes prenom.nom##@uqtr.ca (## garantit l'unicité).
        var users = new List<ApplicationUser>();
        for (var i = 0; i < 40; i++)
        {
            var first = FirstNames[rnd.Next(FirstNames.Length)];
            var last = LastNames[rnd.Next(LastNames.Length)];
            var email = $"{Slugify(first)}.{Slugify(last)}{i:D2}@uqtr.ca";
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                Name = $"{first} {last}",
                // Photo de profil déterministe (pravatar : 70 visages). URL absolue
                // externe : s'affiche telle quelle, indépendante de notre domaine.
                AvatarUrl = $"https://i.pravatar.cc/150?img={(i % 70) + 1}",
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

        var descriptions = new[]
        {
            "Rejoignez-nous pour un moment convivial ouvert à toute la communauté UQTR. Les places sont limitées, inscrivez-vous tôt !",
            "Une belle occasion de bouger, de rencontrer du monde et d'accumuler des cœurs santé.",
            "Activité encadrée par une équipe d'animation d'expérience. Aucun prérequis, tous les niveaux sont bienvenus.",
            "Apportez votre bonne humeur ! Collations et matériel fournis sur place.",
            "Un classique de la vie étudiante à ne pas manquer cette session.",
            "Venez décrocher, ça fait du bien au corps et à la tête. On vous attend en grand nombre !",
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
                description: descriptions[i % descriptions.Length],
                categoryId: category.Id,
                organizerId: organizer.Id,
                startsAt: startsAt,
                endsAt: startsAt.AddHours(2),
                location: "Pavillon Ringuet, UQTR",
                imageUrl: $"https://picsum.photos/seed/act{i}/800/450",
                heartsReward: new[] { 10, 20, 25, 30, 40, 50 }[rnd.Next(6)],
                maxParticipants: new[] { 10, 15, 20, 25, 30 }[rnd.Next(5)],
                // L'URL d'inscription est requise (webview) : une page réelle qui
                // charge, le bouton « J'ai soumis le formulaire » fait le reste.
                registrationUrl: "https://www.uqtr.ca",
                registrationDeadline: startsAt.AddDays(-1),
                isFeatured: i % 5 == 0,
                status: status,
                nowUtc: now,
                participationCost: new[] { 0m, 0m, 10m, 20m }[i % 4]);
            activities.Add(activity);
        }

        // 4 bis) Activité vitrine réelle : « Dîner de la rentrée 2026 » (affiche
        // client). Horaires saisis en heure de Trois-Rivières puis convertis en
        // UTC (convention API), indépendamment du fuseau de la machine de dev.
        var quebec = TimeZoneInfo.FindSystemTimeZoneById("America/Toronto");
        DateTime QuebecUtc(int month, int day, int hour, int minute) =>
            TimeZoneInfo.ConvertTimeToUtc(
                new DateTime(2026, month, day, hour, minute, 0, DateTimeKind.Unspecified),
                quebec);

        activities.Add(Activity.Create(
            title: "Dîner de la rentrée 2026",
            description:
                "Dîner de la rentrée 2026, réservez votre place ! 🍽️\n\n" +
                "Le rendez-vous annuel tant attendu approche ! Rejoignez-nous le " +
                "mardi 25 août 2026 au Campus de Trois-Rivières pour célébrer la " +
                "rentrée entre collègues.\n\n" +
                "📅 Date limite d'inscription : vendredi 14 août 2026 à 17 h\n" +
                "📍 Lieu : Cour intérieure, près du pavillon Pierre-Boucher\n" +
                "🥗 Au menu : Saveurs grecques\n" +
                "💰 Coût : 10 $ (paiement en ligne)\n\n" +
                "👉 Je m'inscris maintenant !\n\n" +
                "⏳ Vous partez bientôt en vacances ? Inscrivez-vous avant de " +
                "partir ! Ne laissez pas la date limite vous dépasser. 😊\n\n" +
                "Vous souhaitez inscrire un collègue actuellement en congé ? " +
                "Écrivez-nous à evenements.CIAS@uqtr.ca et nous nous en occuperons.",
            categoryId: categories[1].Id,  // Socioculturel
            organizerId: organizers[4].Id, // CIAS UQTR
            startsAt: QuebecUtc(8, 25, 12, 0),
            endsAt: QuebecUtc(8, 25, 13, 30),
            location: "Cour intérieure, près du pavillon Pierre-Boucher",
            imageUrl: "https://picsum.photos/seed/diner2026/800/450",
            heartsReward: 20,
            maxParticipants: 120,
            registrationUrl: "https://www.uqtr.ca",
            registrationDeadline: QuebecUtc(8, 14, 17, 0),
            isFeatured: true,
            status: ActivityStatus.Published,
            nowUtc: now,
            participationCost: 10m));

        await _db.Activities.AddRangeAsync(activities, ct);
        await _db.SaveChangesAsync(ct); // activités => Ids

        // 5) Inscriptions + crédits de cœurs.
        //    Activités PASSÉES : remplies (historique réaliste). Activités À VENIR :
        //    on laisse des places libres (40–75 % d'occupation) pour pouvoir
        //    s'inscrire depuis l'app — sauf 1 sur 4, pleine + liste d'attente,
        //    afin de tester aussi les parcours « Complet »/« waitlist ».
        var registrations = 0;
        var heartsCount = 0;
        var futureIndex = 0;
        foreach (var activity in activities.Where(a => a.Status == ActivityStatus.Published))
        {
            var isPast = activity.StartsAt < now;
            var shuffled = users.OrderBy(_ => rnd.Next()).ToList();
            int take;
            if (isPast)
            {
                take = Math.Min(shuffled.Count, activity.MaxParticipants + rnd.Next(0, 5));
            }
            else if (futureIndex++ % 4 == 3)
            {
                // Pleine + 2 à 4 en liste d'attente.
                take = Math.Min(shuffled.Count, activity.MaxParticipants + rnd.Next(2, 5));
            }
            else
            {
                // 40–75 % des places occupées → il reste toujours de la place.
                take = Math.Min(shuffled.Count,
                    Math.Max(1, activity.MaxParticipants * rnd.Next(40, 76) / 100));
            }
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
            "Contenu inapproprié.", "Propos déplacés.",
            "Spam / hors-sujet.", "Harcèlement présumé.",
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
