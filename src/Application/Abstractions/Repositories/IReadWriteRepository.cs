using BldLeague.Domain.Interfaces;

namespace BldLeague.Application.Abstractions.Repositories;

/// <summary>
/// Generic repository interface for performing CRUD operations on entities.
/// Defines methods for retrieving, adding, updating, and deleting entities.
/// </summary>
/// <typeparam name="T">Entity type implementing the IIdentifiable interface, ensuring a unique identifier is available.</typeparam>
public interface IReadWriteRepository<T> : IReadRepository<T> where T : class, IIdentifiable
{
    /// <summary>
    /// Asynchronously adds a new entity of the specified type to the underlying data source.
    /// </summary>
    /// <param name="entity">The entity of type <typeparamref name="T"/> to be added.</param>
    /// <returns>
    /// A Task representing the asynchronous operation.
    /// </returns>
    Task AddAsync(T entity);

    /// <summary>
    /// Asynchronously adds a collection of entities to the underlying data source.
    /// </summary>
    /// <param name="entities">The collection of entities to add.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// </returns>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates the specified entity of type <typeparamref name="T"/> in the underlying data source.
    /// </summary>
    /// <param name="entity">The entity of type <typeparamref name="T"/> to be updated.</param>
    void Update(T entity);

    /// <summary>
    /// Removes the specified entity from the underlying data source.
    /// </summary>
    /// <param name="entity">The entity of type <typeparamref name="T"/> to be deleted.</param>
    void Delete(T entity);
}
