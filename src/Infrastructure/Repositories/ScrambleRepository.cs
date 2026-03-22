using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class ScrambleRepository(AppDbContext context)
    : ReadWriteRepositoryBase<Scramble>(context), IScrambleRepository
{
    public async Task<IReadOnlyCollection<Scramble>> GetByRoundIdAsync(Guid roundId)
        => await DbSet
            .Where(s => s.RoundId == roundId)
            .OrderBy(s => s.ScrambleNumber)
            .ToListAsync();

    public async Task DeleteByRoundIdAsync(Guid roundId)
    {
        var scrambles = await DbSet.Where(s => s.RoundId == roundId).ToListAsync();
        DbSet.RemoveRange(scrambles);
    }
}
