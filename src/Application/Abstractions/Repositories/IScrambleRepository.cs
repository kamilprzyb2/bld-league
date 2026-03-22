using BldLeague.Domain.Entities;

namespace BldLeague.Application.Abstractions.Repositories;

public interface IScrambleRepository : IReadWriteRepository<Scramble>
{
    Task<IReadOnlyCollection<Scramble>> GetByRoundIdAsync(Guid roundId);
    Task DeleteByRoundIdAsync(Guid roundId);
}
