using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Infrastructure.Context;
using BldLeague.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BldLeague.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBldLeagueInfrastructure(this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContextFactory<AppDbContext>(
            options => options.UseNpgsql(connectionString, b => b.MigrationsHistoryTable("__ef_migrations_history")).UseSnakeCaseNamingConvention());
        
        services.AddScoped<IUnitOfWork, IUnitOfWork>(
            provider => new UnitOfWork(provider.GetRequiredService<IDbContextFactory<AppDbContext>>()));
        
        return services;
    }
}