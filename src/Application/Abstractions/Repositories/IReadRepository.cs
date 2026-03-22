using System.Linq.Expressions;
using BldLeague.Domain.Interfaces;

namespace BldLeague.Application.Abstractions.Repositories;

/// <summary>
/// Generic repository interface for performing read operations on entities.
/// Defines methods for retrieving entities.
/// </summary>
/// <typeparam name="T">Entity type implementing the IIdentifiable interface, ensuring a unique identifier is available.</typeparam>
public interface IReadRepository<T> where T : class, IIdentifiable
{
    /// <summary>
    /// Asynchronously checks if an entity with the specified unique identifier exists in the underlying data source.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to check for existence.</param>
    /// <returns>
    /// A Task representing the asynchronous operation, containing a boolean value where true indicates
    /// the entity exists, and false indicates it does not.
    /// </returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Asynchronously checks if any entity in the underlying data source satisfies the specified condition.
    /// </summary>
    /// <param name="predicate">An expression that defines the condition to check for the existence of an entity.</param>
    /// <returns>
    /// A Task representing the asynchronous operation, containing a boolean value where true indicates
    /// at least one entity satisfies the specified condition, and false indicates no entity does.
    /// </returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Retrieves all entities of the specified type.
    /// The method asynchronously fetches all records from the underlying data source.
    /// </summary>
    /// <returns>
    /// A Task representing the asynchronous operation,
    /// containing a list of all entities of type <typeparamref name="T"/>.
    /// </returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Retrieves all entities of the specified type <typeparamref name="T"/> from the data source,
    /// ordered by the specified property.
    /// </summary>
    /// <typeparam name="TKey">
    /// The type of the property used for ordering.
    /// </typeparam>
    /// <param name="orderBy">
    /// An expression specifying the property to order the results by.
    /// </param>
    /// <param name="descending">
    /// Indicates whether the ordering should be descending.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
    /// containing a list of all entities of type <typeparamref name="T"/> ordered according to the provided expression.
    /// </returns>
    Task<List<T>> GetAllAsync<TKey>(
        Expression<Func<T, TKey>> orderBy,
        bool descending = true
    );
    
    /// <summary>
    /// Retrieves a single entity of the specified type by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <returns>
    /// A Task representing the asynchronous operation, containing the entity of type <typeparamref name="T"/> if found;
    /// otherwise, null.
    /// </returns>
    Task<T?> GetByIdAsync(Guid id);
}