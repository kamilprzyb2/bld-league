using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Infrastructure.Context;
using BldLeague.Infrastructure.HostedServices;
using BldLeague.Infrastructure.Options;
using BldLeague.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BldLeague.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBldLeagueInfrastructure(this IServiceCollection services,
        string connectionString, IConfiguration configuration)
    {
        services.AddDbContextFactory<AppDbContext>(
            options => options.UseNpgsql(connectionString, b => b.MigrationsHistoryTable("__ef_migrations_history")).UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, IUnitOfWork>(
            provider => new UnitOfWork(provider.GetRequiredService<IDbContextFactory<AppDbContext>>()));

        services.Configure<RoundFinalizationOptions>(configuration.GetSection(RoundFinalizationOptions.SectionName));
        services.AddHostedService<RoundStandingsRefreshBackgroundService>();

        return services;
    }
}