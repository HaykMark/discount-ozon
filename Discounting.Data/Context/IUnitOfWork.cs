using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Discounting.Common.Types;
using Microsoft.EntityFrameworkCore;

namespace Discounting.Data.Context
{
   public interface IUnitOfWork : IDisposable
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : class;
        Task<TEntity> GetOrFailAsync<TEntity, TKey>(TKey id, IQueryable<TEntity> query = null)
            where TEntity : class, IEntity<TKey>;
        Task<TEntity> GetOrFailAsync<TEntity>(IQueryable<TEntity> query)
            where TEntity : class;
            
        Task<TEntity> AddAsync<TEntity>(TEntity entity) where TEntity : class;

        Task<TEntity> AddAndSaveAsync<TEntity>(TEntity entity) where TEntity : class;
        void AddRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        Task<IEnumerable<TEntity>> AddRangeAndSaveAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        void UpdateRange(IEnumerable<object> entities);
        void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        // Task<TEntity> UpdateAndSaveAsync<TEntity, TKey>(TEntity entity)
        //     where TEntity : class, IEntity<TKey>;

        Task<TEntity> UpdateAndSaveAsync<TEntity, TKey>(TEntity entity,
            params Expression<Func<TEntity, object>>[] excludedProperties)
            where TEntity : class, IEntity<TKey>;
            
        Task<TEntity> UpdateOrCreateAndSaveAsync<TEntity, TKey>(TEntity entity)
            where TEntity : class, IEntity<TKey>;
        Task RemoveAndSaveAsync<TEntity, TKey>(TKey id) where TEntity : class, IEntity<TKey>, new();
        Task RemoveRangeAndSaveAsync<TEntity, TKey>(TKey[] id) where TEntity : class, IEntity<TKey>, new();

        Task UpdateNestedCollectionAsync<TEntity>(IEnumerable<TEntity> entity,
            Expression<Func<TEntity, bool>> condition, bool saveChanges = false)
            where TEntity : class, IEntity<Guid>, new();

        int SaveChanges(bool acceptAllChangesOnSuccess);
        int SaveChanges();

        Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken());

        void DetachLocal<TEntity, TKey>(TEntity entity, TKey id) 
            where TEntity : class, IEntity<TKey>;
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken());
    }
}