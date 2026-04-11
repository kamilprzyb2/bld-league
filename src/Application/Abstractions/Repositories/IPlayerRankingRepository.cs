using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IPlayerRankingRepository : IReadWriteRepository<PlayerRanking>
{
    Task<IReadOnlyCollection<PlayerRanking>> GetAllWithDetailsAsync();

    Task DeleteAllAsync();
}
