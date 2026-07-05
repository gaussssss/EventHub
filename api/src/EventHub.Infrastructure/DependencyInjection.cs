using EventHub.Domain.Services;
using EventHub.Domain.Repositories;
using EventHub.Application.Activities;
using EventHub.Application.Common.Messaging;
using EventHub.Application.Hearts;
using EventHub.Application.Moderation;
using EventHub.Application.Profile;
using EventHub.Application.Registrations;
using EventHub.Application.Social;
using EventHub.Application.Users;
using EventHub.Infrastructure.Common;
using EventHub.Infrastructure.Identity;
using EventHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("Default") ?? "Data Source=eventhub.db";

        services.AddDbContext<EventHubDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<EventHubDbContext>();

        services.AddScoped<IActivityReadRepository, ActivityReadRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<ICategoryReadRepository, CategoryReadRepository>();
        services.AddScoped<IOrganizerRepository, OrganizerRepository>();
        services.AddScoped<IOrganizerReadRepository, OrganizerReadRepository>();
        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IRegistrationReadRepository, RegistrationReadRepository>();
        services.AddScoped<IHeartReadRepository, HeartReadRepository>();
        services.AddScoped<ILeaderboardReadRepository, LeaderboardReadRepository>();
        services.AddScoped<IDashboardReadRepository, DashboardReadRepository>();
        services.AddScoped<IHeartTransactionRepository, HeartTransactionRepository>();
        services.AddScoped<IPostReadRepository, PostReadRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUserReadRepository, UserReadRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportReadRepository, ReportReadRepository>();
        services.AddScoped<IUserAdminReadRepository, UserAdminReadRepository>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationReadRepository, NotificationReadRepository>();
        services.AddScoped<IPushSender, LoggingPushSender>();
        services.AddSingleton<IStorageService, StubStorageService>();
        services.AddScoped<IGamificationSettingsRepository, GamificationSettingsRepository>();
        services.AddSingleton<IClock, SystemClock>();

        // Seeder de données de développement (utilisé par l'endpoint dev-only).
        services.AddScoped<Dev.DevDataSeeder>();

        // CQRS : médiateur + découverte automatique des handlers de la couche Application.
        services.AddScoped<ISender, Sender>();
        AddApplicationHandlers(services);

        return services;
    }

    /// <summary>
    /// Scanne l'assembly Application et enregistre chaque implémentation de
    /// <see cref="ICommandHandler{TCommand,TResult}"/> / <see cref="IQueryHandler{TQuery,TResult}"/>
    /// sous son interface fermée — aucun registre message→handler à maintenir.
    /// </summary>
    private static void AddApplicationHandlers(IServiceCollection services)
    {
        var assembly = typeof(ISender).Assembly;
        var openHandlers = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };

        foreach (var type in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false }))
        {
            foreach (var contract in type.GetInterfaces().Where(i =>
                         i.IsGenericType && openHandlers.Contains(i.GetGenericTypeDefinition())))
            {
                services.AddScoped(contract, type);
            }
        }
    }
}
