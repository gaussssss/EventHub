using EventHub.Domain.Entities;
using EventHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence;

public class EventHubDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public EventHubDbContext(DbContextOptions<EventHubDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<HeartTransaction> HeartTransactions => Set<HeartTransaction>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<PostLike> PostLikes => Set<PostLike>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationSettings> NotificationSettings => Set<NotificationSettings>();
    public DbSet<GamificationSettings> GamificationSettings => Set<GamificationSettings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Un compte Entra ↔ un utilisateur interne (les NULL restent distincts,
        // donc les comptes hors SSO ne se gênent pas).
        builder.Entity<ApplicationUser>(e =>
        {
            e.HasIndex(u => u.EntraObjectId).IsUnique();
        });

        builder.Entity<Category>(e =>
        {
            e.HasIndex(c => c.Slug).IsUnique();
        });

        builder.Entity<Activity>(e =>
        {
            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.Version).IsConcurrencyToken();
            e.HasIndex(a => a.StartsAt);
            e.HasOne(a => a.Category).WithMany().HasForeignKey(a => a.CategoryId);
            e.HasOne(a => a.Organizer).WithMany().HasForeignKey(a => a.OrganizerId);
        });

        builder.Entity<Registration>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>();
            e.HasIndex(r => new { r.UserId, r.ActivityId }).IsUnique();
            e.HasOne(r => r.Activity).WithMany().HasForeignKey(r => r.ActivityId);
        });

        builder.Entity<HeartTransaction>(e =>
        {
            e.HasIndex(h => h.UserId);
        });

        builder.Entity<Post>(e =>
        {
            e.HasIndex(p => p.CreatedAt);
            e.HasMany(p => p.Comments).WithOne().HasForeignKey(c => c.PostId);
            e.HasMany(p => p.Likes).WithOne().HasForeignKey(l => l.PostId);
            // Collections encapsulées (champs _comments/_likes) : accès par champ.
            e.Navigation(p => p.Comments).UsePropertyAccessMode(PropertyAccessMode.Field);
            e.Navigation(p => p.Likes).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Entity<PostLike>(e =>
        {
            e.HasKey(l => new { l.PostId, l.UserId });
        });

        builder.Entity<Report>(e =>
        {
            e.Property(r => r.TargetType).HasConversion<string>();
            e.Property(r => r.Status).HasConversion<string>();
            e.HasIndex(r => new { r.TargetType, r.TargetId });
        });

        builder.Entity<Device>(e =>
        {
            e.HasIndex(d => d.UserId);
            e.HasIndex(d => d.PushToken).IsUnique();
        });

        builder.Entity<Notification>(e =>
        {
            e.HasIndex(n => new { n.UserId, n.CreatedAt });
        });

        builder.Entity<NotificationSettings>(e =>
        {
            e.HasKey(s => s.UserId);
        });
    }
}
