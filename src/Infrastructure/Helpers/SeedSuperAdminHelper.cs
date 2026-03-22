using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BldLeague.Infrastructure.Helpers;

public static class SeedSuperAdminHelper
{
    public static async Task SeedAsync<TDbContext>(IServiceProvider serviceProvider, IConfiguration configuration)
        where TDbContext : AppDbContext
    {
        var wcaId = configuration["SuperAdmin:WcaId"];
        var fullName = configuration["SuperAdmin:FullName"];

        if (string.IsNullOrWhiteSpace(wcaId))
            return;

        if (string.IsNullOrWhiteSpace(fullName))
            fullName = wcaId;

        await using var scope = serviceProvider.CreateAsyncScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TDbContext>>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
        await using var db = await factory.CreateDbContextAsync();

        var users = db.Set<User>();

        var existing = await users.FirstOrDefaultAsync(u => u.WcaId == wcaId);

        if (existing == null)
        {
            await users.AddAsync(User.Create(fullName, wcaId, isAdmin: true));
            await db.SaveChangesAsync();
            logger.LogInformation("Super admin '{FullName}' ({WcaId}) created.", fullName, wcaId);
        }
        else if (!existing.IsAdmin)
        {
            await users
                .Where(u => u.WcaId == wcaId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.IsAdmin, true));
            logger.LogInformation("Super admin '{FullName}' ({WcaId}) promoted to admin.", existing.FullName, wcaId);
        }
    }
}
