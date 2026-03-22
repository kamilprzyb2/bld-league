using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BldLeague.Infrastructure.Helpers;

/// <summary>
/// Provides a helper method to ensure that the database schema is migrated to the latest version.
/// </summary>
public static class EnsureMigratedHelper
{
    /// <summary>
    /// Ensures that the database schema for the specified DbContext is migrated to the latest version.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext for which the migration should be ensured.</typeparam>
    /// <param name="serviceProvider">An instance of the service provider used to create the scope and resolve the DbContext.</param>
    /// <returns>A task representing the asynchronous operation of migrating the database schema.</returns>
    public static async Task EnsureMigratedAsync<TDbContext>(IServiceProvider serviceProvider)
        where TDbContext : DbContext
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();

        await dbContext.Database.MigrateAsync();
    }
}
