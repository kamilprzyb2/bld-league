using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class PlayerRankingRepository(AppDbContext context)
    : ReadWriteRepositoryBase<PlayerRanking>(context), IPlayerRankingRepository
{
    public async Task<IReadOnlyCollection<PlayerRanking>> GetAllWithDetailsAsync()
    {
        return await DbSet
            .Include(pr => pr.User)
            .Include(pr => pr.SingleRound)
                .ThenInclude(r => r!.Season)
            .Include(pr => pr.AverageRound)
                .ThenInclude(r => r!.Season)
            .ToListAsync();
    }
}
