using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IPlayerRankingRepository : IReadWriteRepository<PlayerRanking>
{
    Task<PlayerRanking?> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyCollection<PlayerRanking>> GetAllWithDetailsAsync();
}
