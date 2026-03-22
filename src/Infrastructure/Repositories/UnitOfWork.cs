using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

/// <summary>
/// The UnitOfWork class serves as an implementation of the Unit of Work pattern, coordinating a set of repositories
/// for managing domain entities to ensure changes are committed as a single unit of work. It deals with multiple
/// entity types in a consistent transaction, leveraging lazy-loading for repository instantiation.
/// </summary>
public class UnitOfWork : IUnitOfWork, IDisposable, IAsyncDisposable
{
    /// <summary>
    /// A private instance of the database context.
    /// </summary>
    private readonly AppDbContext _dbContext;
    
    // Lazy-loaded repositories for various entities.
    private readonly Lazy<IUserRepository> _userRepository;
    private readonly Lazy<ILeagueRepository> _leagueRepository;
    private readonly Lazy<ISeasonRepository> _seasonRepository;
    private readonly Lazy<ILeagueSeasonRepository> _leagueSeasonRepository;
    private readonly Lazy<ILeagueSeasonUserRepository> _leagueSeasonUserRepository;
    private readonly Lazy<IRoundRepository> _roundRepository;
    private readonly Lazy<IMatchRepository> _matchRepository;
    private readonly Lazy<ISolveRepository> _solveRepository;
    private readonly Lazy<IScrambleRepository> _scrambleRepository;
    private readonly Lazy<IRoundStandingRepository> _roundStandingRepository;
    private readonly Lazy<ILeagueSeasonStandingRepository> _leagueSeasonStandingRepository;

    // Public properties to access the lazily loaded repositories.
    public IUserRepository UserRepository => _userRepository.Value;
    public ILeagueRepository LeagueRepository => _leagueRepository.Value;
    public ISeasonRepository SeasonRepository => _seasonRepository.Value;
    public ILeagueSeasonRepository LeagueSeasonRepository => _leagueSeasonRepository.Value;
    public ILeagueSeasonUserRepository LeagueSeasonUserRepository => _leagueSeasonUserRepository.Value;
    public IRoundRepository RoundRepository => _roundRepository.Value;
    public IMatchRepository MatchRepository => _matchRepository.Value;
    public ISolveRepository SolveRepository => _solveRepository.Value;
    public IScrambleRepository ScrambleRepository => _scrambleRepository.Value;
    public IRoundStandingRepository RoundStandingRepository => _roundStandingRepository.Value;
    public ILeagueSeasonStandingRepository LeagueSeasonStandingRepository => _leagueSeasonStandingRepository.Value;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// Creates a new database context and initializes lazy-loaded repositories with the context.
    /// </summary>
    /// <param name="dbContextFactory">The factory to create the database context.</param>
    public UnitOfWork(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContext = dbContextFactory.CreateDbContext();
        
        _userRepository = 
            new Lazy<IUserRepository>(() => new UserRepository(_dbContext));
        _leagueRepository = 
            new Lazy<ILeagueRepository>(() => new LeagueRepository(_dbContext));
        _seasonRepository = 
            new Lazy<ISeasonRepository>(() => new SeasonRepository(_dbContext));
        _leagueSeasonRepository = 
            new Lazy<ILeagueSeasonRepository>(() => new LeagueSeasonRepository(_dbContext));
        _leagueSeasonUserRepository = 
            new Lazy<ILeagueSeasonUserRepository>(() => new LeagueSeasonUserRepository(_dbContext));
        _roundRepository =
            new Lazy<IRoundRepository>(() => new RoundRepository(_dbContext));
        _matchRepository=
            new Lazy<IMatchRepository>(() => new MatchRepository(_dbContext));
        _solveRepository =
            new  Lazy<ISolveRepository>(() => new SolveRepository(_dbContext));
        _scrambleRepository =
            new Lazy<IScrambleRepository>(() => new ScrambleRepository(_dbContext));
        _roundStandingRepository =
            new Lazy<IRoundStandingRepository>(() => new RoundStandingRepository(_dbContext));
        _leagueSeasonStandingRepository =
            new  Lazy<ILeagueSeasonStandingRepository>(() => new LeagueSeasonStandingRepository(_dbContext));
    }

    /// <inheritdoc />
    public async Task<int> SaveAsync()
    {
        var result = await _dbContext.SaveChangesAsync();
        return result;
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync()
    {
        await _dbContext.Database.BeginTransactionAsync();
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync()
    {
        await _dbContext.Database.CommitTransactionAsync();
    }

    /// <summary>
    /// Disposes the database context and suppresses finalization.
    /// </summary>
    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes the database context and suppresses finalization.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}