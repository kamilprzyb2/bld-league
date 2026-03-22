using System.Linq.Expressions;
using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

/// <summary>
/// Abstract base class for read-only repositories.
/// </summary>
/// <typeparam name="T">The entity type that the repository will manage, constrained to be a class.</typeparam>
public abstract class ReadRepositoryBase<T>(DbContext context) : IReadRepository<T>
    where T : class, IIdentifiable
{
    /// <summary>
    /// The DbSet representing the entity type in the database context.
    /// </summary>
    protected readonly DbSet<T> DbSet = context.Set<T>();

    /// <inhertidoc />
    public async Task<bool> ExistsAsync(Guid id)
        => await DbSet.IgnoreQueryFilters().AnyAsync(e => e.Id == id);

    /// <inhertidoc />
    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        => await DbSet.IgnoreQueryFilters().AnyAsync(predicate);

    /// <inhertidoc />
    public async Task<List<T>> GetAllAsync()
        => await DbSet.ToListAsync();

    /// <inhertidoc />
    public async Task<List<T>> GetAllAsync<TKey>(
        Expression<Func<T, TKey>> orderBy,
        bool descending = true
    )
    {
        IQueryable<T> query = DbSet;
        
        query = descending
            ? query.OrderByDescending(orderBy)
            : query.OrderBy(orderBy);
            
        return await query.ToListAsync();
    }
    
    /// <inhertidoc />
    public async Task<T?> GetByIdAsync(Guid id) 
        => await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id);
}
