using EFCore.BulkExtensions;
using IceSync.Domain.Exceptions.Custom;
using IceSync.Domain.Interfaces;
using IceSync.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IceSync.Infrastructure.Repositories;

public class BaseDbRepository<TEntity, TContext> : IBaseRepository<TEntity>
        where TEntity : class, IEntity
        where TContext : DbContext, new()
{
    private readonly TContext _currentContext;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseDbRepository(TContext currentContext)
    {
        _currentContext = currentContext ?? throw new ArgumentNullException(nameof(TContext));
        _dbSet = _currentContext.Set<TEntity>();
    }

    public async virtual Task<List<TEntity>> Index(bool trackEntities = false, CancellationToken cancellationToken = default)
    {
        var entities = _dbSet.AsQueryable();

        if (!trackEntities)
            entities = entities.AsNoTracking();

        return await entities.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async virtual Task<List<TEntity>> Index(Expression<Func<TEntity, bool>> predicate, bool trackEntities = false, CancellationToken cancellationToken = default)
    {
        CheckPredicate(predicate);

        var entities = _dbSet.AsQueryable();

        if (!trackEntities)
            entities = entities.AsNoTracking();

        return await entities.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<TEntity> Get(Expression<Func<TEntity, bool>> predicate, bool trackEntities = false, CancellationToken cancellationToken = default)
    {
        CheckPredicate(predicate);

        var entities = _dbSet.AsQueryable();

        if (!trackEntities)
            entities = entities.AsNoTracking();

        return await entities.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);
    }

    public virtual TEntity Add(TEntity entity)
    {
        CheckEntity(entity);

        _currentContext.Add(entity);
        return entity;
    }

    public virtual TEntity Update(TEntity entity)
    {
        CheckEntity(entity);

        _currentContext.Attach(entity);
        _currentContext.Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public async virtual Task<TEntity> Delete(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        CheckPredicate(predicate);

        var entity = await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

        if (entity == null)
        {
            throw new NotFoundDomainException(
                $"object of type {typeof(TEntity)} couldn't be found.");
        }

        _dbSet.Remove(entity);
        return entity;
    }

    public async virtual Task BulkInsert(IList<TEntity> entities, CancellationToken cancellationToken)
    {
        CheckEntityCollection(entities);

        await _currentContext.BulkInsertAsync(entities, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async virtual Task BulkUpdate(IList<TEntity> entities, CancellationToken cancellationToken)
    {
        CheckEntityCollection(entities);

        await _currentContext.BulkUpdateAsync(entities, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async virtual Task BulkDelete(IList<TEntity> entities, CancellationToken cancellationToken)
    {
        CheckEntityCollection(entities);

        await _currentContext.BulkDeleteAsync(entities, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async virtual Task BulkDelete(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        CheckPredicate(predicate);

        var entityCollection = await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        await _currentContext.BulkDeleteAsync(entityCollection, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async virtual Task BulkInsertOrUpdate(IList<TEntity> entities, CancellationToken cancellationToken)
    {
        CheckEntityCollection(entities);

        await _currentContext.BulkInsertOrUpdateAsync(entities, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task SaveChanges(CancellationToken cancellationToken)
    {
        await _currentContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    ///     Checking that predicate isn't null
    /// </summary>
    /// <param name="predicate"></param>
    private void CheckPredicate(Expression<Func<TEntity, bool>> predicate)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }
    }

    /// <summary>
    ///     Check if null entity
    /// </summary>
    /// <param name="entity"></param>
    public void CheckEntity(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }
    }

    /// <summary>
    ///     Check if null entity collection
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="entityCollection"></param>
    public void CheckEntityCollection(IList<TEntity> entityCollection)
    {
        if (entityCollection == null)
        {
            throw new ArgumentNullException(nameof(entityCollection));
        }
    }
}