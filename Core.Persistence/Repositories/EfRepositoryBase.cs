using System;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Core.Persistence.Repositories
{
	public class EfRepositoryBase<TEntity,TEntityId,TContext> : IAsyncRepository<TEntity,TEntityId>,IRepository<TEntity,TEntityId>
		where TEntity: Entity<TEntityId>
		where TContext : DbContext
	{
		protected readonly TContext Context;

        public EfRepositoryBase(TContext context)
        {
            Context = context;
        }

        public async Task<TEntity> AddAsyn(TEntity entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await Context.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities)
        {
            foreach(TEntity entity in entities)
            {
                entity.CreatedDate = DateTime.UtcNow;
                await Context.AddRangeAsync(entities);
                await Context.SaveChangesAsync();
            }
            return entities;
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();
            if (!enableTracking)
            {
                queryable = queryable.AsNoTracking();
            }
            if (withDeleted)
            {
                queryable = queryable.IgnoreQueryFilters();
            }
            if (predicate != null) 
            {
                queryable = queryable.Where(predicate);
            }

            return await queryable.AnyAsync(cancellationToken);
        }

        public async Task<TEntity> DeleteAsync(TEntity entity, bool permanent = false)
        {
            await SetEntityAsDeletedAsync(entity, permanent);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, bool permanent = false)
        {
            await SetEntityAsDeletedAsync(entities, permanent);
            await Context.SaveChangesAsync();

            return entities;
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();
            if (!enableTracking)
            {
                queryable = queryable.AsNoTracking();
            }
            if (include != null)
            {
                queryable = include(queryable); 
            }
            if (withDeleted) 
            {
                queryable = queryable.IgnoreQueryFilters();
            }

            return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
        }

        public async Task<Paginate<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query();
            if (!enableTracking)
            {
                queryable = queryable.AsNoTracking();
            }
            if(include != null)
            {
                queryable = include(queryable); 
            }
            if (withDeleted) 
            {
                queryable = queryable.IgnoreQueryFilters();
            }
            if(predicate != null)
            {
                queryable = queryable.Where(predicate);
            }
            if(orderBy != null)
            {
                return await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken);
            }
            return await queryable.ToPaginateAsync(index, size, cancellationToken);
        }

        public async Task<Paginate<TEntity>> GetListByDynamicAsync(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
        {
            IQueryable<TEntity> queryable = Query().ToDynamic(dynamic);
            if (!enableTracking)
            {
                queryable = queryable.AsNoTracking();
            }
            if (include != null)
            {
                queryable = include(queryable); 
            }
            if (withDeleted) 
            {
                queryable = queryable.IgnoreQueryFilters();
            }
            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }
            return await queryable.ToPaginateAsync(index, size, cancellationToken);
        }

        public IQueryable<TEntity> Query() => Context.Set<TEntity>();

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            entity.UpdatedDate = DateTime.UtcNow;
            Context.Update(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public async Task<ICollection<TEntity>> UpdateRangeAsync(ICollection<TEntity> entities)
        {
            foreach (TEntity entity in entities)
                entity.UpdatedDate = DateTime.UtcNow;
            Context.UpdateRange(entities);
            await Context.SaveChangesAsync();
            return entities;
        }
        #region Helper Method
        protected async Task SetEntityAsDeletedAsync(TEntity entity, bool permanent)
        {
            if (!permanent) 
            {
                CheckHasEntityHaveOneToOneRelation(entity);
                await SetEntityAsSoftDeletedAsync(entity);
            }else
            {
                Context.Remove(entity);
            }
        }

        protected void CheckHasEntityHaveOneToOneRelation(TEntity entity)
        {
            bool hasEntityHaveOneToOneRelation =
            Context
                .Entry(entity)
                .Metadata.GetForeignKeys()
                .All(
                    x =>
                        x.DependentToPrincipal?.IsCollection == true
                        || x.PrincipalToDependent?.IsCollection == true
                        || x.DependentToPrincipal?.ForeignKey.DeclaringEntityType.ClrType == entity.GetType()
                ) == false;
            if (hasEntityHaveOneToOneRelation)
                throw new InvalidOperationException(
                    "Entity has one-to-one relationship. Soft Delete causes problems if you try to create entry again by same foreign key."
                );
        }

        private async Task SetEntityAsSoftDeletedAsync(IEntityTimestamps entity)
        {
            if (entity.DeletedDate.HasValue)
                return;
            entity.DeletedDate = DateTime.UtcNow;

            var navigations = Context
                .Entry(entity)
                .Metadata.GetNavigations()
                .Where(x => x is { IsOnDependent: false, ForeignKey.DeleteBehavior: DeleteBehavior.ClientCascade or DeleteBehavior.Cascade })
                .ToList();
            foreach (INavigation? navigation in navigations)
            {
                if (navigation.TargetEntityType.IsOwned())
                    continue;
                if (navigation.PropertyInfo == null)
                    continue;

                object? navValue = navigation.PropertyInfo.GetValue(entity);
                if (navigation.IsCollection)
                {
                    if (navValue == null)
                    {
                        IQueryable query = Context.Entry(entity).Collection(navigation.PropertyInfo.Name).Query();
                        navValue = await GetRelationLoaderQuery(query, navigationPropertyType: navigation.PropertyInfo.GetType()).ToListAsync();
                        if (navValue == null)
                            continue;
                    }

                    foreach (IEntityTimestamps navValueItem in (IEnumerable)navValue)
                        await SetEntityAsSoftDeletedAsync(navValueItem);
                }
                else
                {
                    if (navValue == null)
                    {
                        IQueryable query = Context.Entry(entity).Reference(navigation.PropertyInfo.Name).Query();
                        navValue = await GetRelationLoaderQuery(query, navigationPropertyType: navigation.PropertyInfo.GetType())
                            .FirstOrDefaultAsync();
                        if (navValue == null)
                            continue;
                    }

                    await SetEntityAsSoftDeletedAsync((IEntityTimestamps)navValue);
                }
            }

            Context.Update(entity);
        }

        protected IQueryable<object> GetRelationLoaderQuery(IQueryable query, Type navigationPropertyType)
        {
            Type queryProviderType = query.Provider.GetType();
            MethodInfo createQueryMethod =
                queryProviderType
                    .GetMethods()
                    .First(m => m is { Name: nameof(query.Provider.CreateQuery), IsGenericMethod: true })
                    ?.MakeGenericMethod(navigationPropertyType)
                ?? throw new InvalidOperationException("CreateQuery<TElement> method is not found in IQueryProvider.");
            var queryProviderQuery =
                (IQueryable<object>)createQueryMethod.Invoke(query.Provider, parameters: new object[] { query.Expression })!;
            return queryProviderQuery.Where(x => !((IEntityTimestamps)x).DeletedDate.HasValue);
        }

        protected async Task SetEntityAsDeletedAsync(IEnumerable<TEntity> entities, bool permanent)
        {
            foreach (TEntity entity in entities)
                await SetEntityAsDeletedAsync(entity, permanent);
        }

        TEntity? IRepository<TEntity, TEntityId>.GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include, bool withDeleted, bool enableTracking, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Paginate<TEntity> IRepository<TEntity, TEntityId>.GetListAsync(Expression<Func<TEntity, bool>>? predicate, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include, int index, int size, bool withDeleted, bool enableTracking, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        Paginate<TEntity> IRepository<TEntity, TEntityId>.GetListByDynamicAsync(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include, int index, int size, bool withDeleted, bool enableTracking, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        bool IRepository<TEntity, TEntityId>.AnyAsync(Expression<Func<TEntity, bool>>? predicate, bool withDeleted, bool enableTracking, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        TEntity IRepository<TEntity, TEntityId>.AddAsyn(TEntity entity)
        {
            throw new NotImplementedException();
        }

        ICollection<TEntity> IRepository<TEntity, TEntityId>.AddRangeAsync(ICollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        TEntity IRepository<TEntity, TEntityId>.UpdateAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        ICollection<TEntity> IRepository<TEntity, TEntityId>.UpdateRangeAsync(ICollection<TEntity> entities)
        {
            throw new NotImplementedException();
        }

        TEntity IRepository<TEntity, TEntityId>.DeleteAsync(TEntity entity, bool permanent)
        {
            throw new NotImplementedException();
        }

        ICollection<TEntity> IRepository<TEntity, TEntityId>.DeleteRangeAsync(ICollection<TEntity> entities, bool permanent)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

