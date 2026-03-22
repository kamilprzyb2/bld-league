using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Application.Queries.Seasons.GetAll;
using BldLeague.Domain.Entities;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

public class SeasonRepository(AppDbContext context) :
    ReadWriteRepositoryBase<Season>(context), ISeasonRepository
{
    public async Task<int?> GetLatestSeasonNumberAsync()
        => await DbSet
            .Select(s => (int?)s.SeasonNumber)
            .MaxAsync();

    public async Task<Season?> GetBySeasonNumberAsync(int seasonNumber)
        => await DbSet.FirstOrDefaultAsync(s => s.SeasonNumber == seasonNumber);

    public async Task<IReadOnlyCollection<SeasonSummaryDto>> GetAllSummariesAsync()
        => await DbSet
            .OrderByDescending(s => s.SeasonNumber)
            .Select(s => new SeasonSummaryDto(s.Id, s.SeasonNumber))
            .ToListAsync();

    public async Task<SeasonSummaryDto?> GetSummaryByIdAsync(Guid id)
        => await DbSet
            .Where(s => s.Id == id)
            .Select(s => new SeasonSummaryDto(s.Id, s.SeasonNumber))
            .FirstOrDefaultAsync();
}
