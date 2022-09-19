using System.Linq.Expressions;

namespace IceSync.Domain.Interfaces.Repositories;
public interface IBaseRepository<TEntity> where TEntity : class, IEntity
{
    TEntity Add(TEntity entity);

    Task<List<TEntity>> Index(bool trackEntities = false, CancellationToken cancellationToken = default);
    Task<List<TEntity>> Index(Expression<Func<TEntity, bool>> predicate, bool trackEntities = false, CancellationToken cancellationToken = default);

    Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate, bool trackEntities = false, CancellationToken cancellationToken = default);

    TEntity Update(TEntity entity);
    Task<TEntity> Delete(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task BulkInsert(IList<TEntity> entities, CancellationToken cancellationToken);
    Task BulkUpdate(IList<TEntity> entities, CancellationToken cancellationToken);
    Task BulkDelete(IList<TEntity> entities, CancellationToken cancellationToken);
    Task BulkDelete(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task BulkInsertOrUpdate(IList<TEntity> entities, CancellationToken cancellationToken);

    Task SaveChanges(CancellationToken cancellationToken);
}