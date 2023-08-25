using System;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Core.Persistence.Paging;
using Core.Persistence.Dynamic;

namespace Core.Persistence.Repositories
{
	public interface IRepository<TEntity,TEntityId> : IQuery<TEntity> where TEntity : Entity<TEntityId>
	{
        TEntity? GetAsync(Expression<Func<TEntity, bool>> predicate,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            bool withDeleted = false,
            bool enableTracking = true,
            CancellationToken cancellationToken = default);//join atmamızı sağlayacak 2. parametrede yazdığımız, db de silinenleri sorgularda getireyimmi demek 3.parametredeki,
                                                           //cancellationToken - asenktron operasyonlarda iptal etmek için

        Paginate<TEntity> GetListAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            int index = 0,
            int size = 10,
            bool withDeleted = false,
            bool enableTracking = true,
            CancellationToken cancellationToken = default
            );

        Paginate<TEntity> GetListByDynamicAsync(
            DynamicQuery dynamic,
            Expression<Func<TEntity, bool>>? predicate = null,
            Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
            int index = 0,
            int size = 10,
            bool withDeleted = false,
            bool enableTracking = true,
            CancellationToken cancellationToken = default
            );

        bool AnyAsync(
            Expression<Func<TEntity, bool>>? predicate = null,
            bool withDeleted = false,
            bool enableTracking = true,
            CancellationToken cancellationToken = default);

        TEntity AddAsyn(TEntity entity);

        ICollection<TEntity> AddRangeAsync(ICollection<TEntity> entities);

        TEntity UpdateAsync(TEntity entity);

        ICollection<TEntity> UpdateRangeAsync(ICollection<TEntity> entities);

        TEntity DeleteAsync(TEntity entity, bool permanent = false);

        ICollection<TEntity> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false);
    }
}

