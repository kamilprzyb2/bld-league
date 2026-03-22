using BldLeague.Application.Abstractions.Repositories;
using BldLeague.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BldLeague.Infrastructure.Repositories;

/// <summary>
/// Abstract base class for read-write repositories.
/// </summary>
/// <typeparam name="T">The entity type that the repository will manage, constrained to be a class.</typeparam>
public abstract class ReadWriteRepositoryBase<T>(DbContext context) : ReadRepositoryBase<T>(context), IReadWriteRepository<T>
    where T : class, IIdentifiable
{
    /// <inhertidoc />
    public async Task AddAsync(T entity)
        => await DbSet.AddAsync(entity);

    /// <inhertidoc />
    public async Task AddRangeAsync(IEnumerable<T> entities)
        => await DbSet.AddRangeAsync(entities);

    /// <inhertidoc />
    public void Update(T entity)
        => DbSet.Update(entity);

    /// <inhertidoc />
    public void Delete(T entity)
        => DbSet.Remove(entity);
}