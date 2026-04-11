namespace BldLeague.Application.Abstractions.Repositories;

/// <summary>
/// Represents the unit of work pattern interface, encapsulating a set of repositories and a commit mechanism.
/// Provides access to various repositories and ensures changes are committed as a single transaction.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ILeagueRepository LeagueRepository { get; }
    ISeasonRepository SeasonRepository { get; }
    ILeagueSeasonRepository LeagueSeasonRepository { get; }
    ILeagueSeasonUserRepository LeagueSeasonUserRepository { get; }
    IRoundRepository RoundRepository { get; }
    IMatchRepository MatchRepository { get; }
    ISolveRepository SolveRepository { get; }
    IScrambleRepository ScrambleRepository { get; }
    IRoundStandingRepository RoundStandingRepository { get; }
    ILeagueSeasonStandingRepository LeagueSeasonStandingRepository { get; }
    IPlayerRankingRepository PlayerRankingRepository { get; }
    
    /// <summary>
    /// Commits all changes made in the current unit of work as a single transaction asynchronously.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveAsync();

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task BeginTransactionAsync();

    /// <summary>
    /// Finalizes and saves all changes in the current database transaction asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CommitTransactionAsync();
}