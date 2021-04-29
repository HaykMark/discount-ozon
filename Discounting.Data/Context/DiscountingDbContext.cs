using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Discounting.Common.Exceptions;
using Discounting.Common.Types;
using Discounting.Entities.Account;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Discounting.Data.Context
{
    public class DiscountingDbContext : DbContext, IUnitOfWork
    {
        public DiscountingDbContext(DbContextOptions<DiscountingDbContext> options)
            : base(options)
        {
            base.ChangeTracker
                .QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
        }

        /// This method is needed due to an issue where entities are tracked that occurred in .net core v3
        /// For some reason ef core tracks some changes even if we explicitly disabled tracking.
        /// TODO:  Try to find a better solution and remove this method
        public void DetachLocal<TEntity, TKey>(TEntity entity, TKey id)
            where TEntity : class, IEntity<TKey>
        {
            var local = Set<TEntity>()
                .Local
                .FirstOrDefault(entry => entry.Id.Equals(id));
            if (local != null)
            {
                Entry(local).State = EntityState.Detached;
            }

            Entry(entity).State = EntityState.Modified;
        }

        public async Task<TEntity> GetOrFailAsync<TEntity, TKey>(
            TKey id,
            IQueryable<TEntity> baseQuery = null
        ) where TEntity : class, IEntity<TKey>
        {
            baseQuery ??= Set<TEntity>();

            baseQuery = baseQuery.Where(e => e.Id.Equals(id));

            return await GetOrFailAsync(baseQuery);
        }

        public async Task<TEntity> GetOrFailAsync<TEntity>(
            IQueryable<TEntity> query
        ) where TEntity : class
        {
            var entity = await query.FirstOrDefaultAsync();
            if (entity == null)
            {
                throw new NotFoundException(typeof(TEntity));
            }

            return entity;
        }

        public async Task<TEntity> AddAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            await Set<TEntity>().AddAsync(entity);
            return entity;
        }

        public async Task<TEntity> AddAndSaveAsync<TEntity>(TEntity entity)
            where TEntity : class
        {
            await Set<TEntity>().AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        public void AddRange<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            Set<TEntity>().AddRange(entities);
        }

        public async Task<IEnumerable<TEntity>> AddRangeAndSaveAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : class
        {
            await Set<TEntity>().AddRangeAsync(entities);
            await SaveChangesAsync();
            return entities;
        }

        // public async Task<TEntity> UpdateAndSaveAsync<TEntity, TKey>(TEntity entity)
        //     where TEntity : class, IEntity<TKey>
        // {
        //     // TODO: improve: this will fail if a composite key is used in ef core config
        //     if (!await Set<TEntity>().AnyAsync(e => e.Id.Equals(entity.Id)))
        //     {
        //         throw new NotFoundException(typeof(TEntity));
        //     }
        //
        //     Set<TEntity>().Update(entity);
        //     await SaveChangesAsync();
        //     return entity;
        // }

        public async Task<TEntity> UpdateAndSaveAsync<TEntity, TKey>(TEntity entity,
            params Expression<Func<TEntity, object>>[] excludedProperties)
            where TEntity : class, IEntity<TKey>
        {
            // TODO: improve: this will fail if a composite key is used in ef core config
            if (!await Set<TEntity>().AnyAsync(e => e.Id.Equals(entity.Id)))
            {
                throw new NotFoundException(typeof(TEntity));
            }

            if (excludedProperties.Any())
            {
                Entry(entity).State = EntityState.Modified;
                foreach (var property in excludedProperties)
                {
                    Entry(entity).Property(property).IsModified = false;
                }
                await SaveChangesAsync();
                return entity;
            }

            Set<TEntity>().Update(entity);
            await SaveChangesAsync();
            return entity;
        }

        public async Task<TEntity> UpdateOrCreateAndSaveAsync<TEntity, TKey>(TEntity entity)
            where TEntity : class, IEntity<TKey>
        {
            // TODO: improve: this will fail if a composite key is used in ef core config
            if (await Set<TEntity>().AnyAsync(e => e.Id.Equals(entity.Id)))
            {
                return await UpdateAndSaveAsync<TEntity, TKey>(entity);
            }

            return await AddAndSaveAsync(entity);
        }

        public async Task RemoveAndSaveAsync<TEntity, TKey>(TKey id)
            where TEntity : class, IEntity<TKey>, new()
        {
            var entity = new TEntity {Id = id};
            Remove(entity);
            try
            {
                await SaveChangesAsync();
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception)
            {
                var key = entity is IActivatable
                    ? "cannot-delete-set-to-inactive"
                    : "cannot-delete";
                throw new EntityReferenceException(key);
            }
        }

        public async Task RemoveRangeAndSaveAsync<TEntity, TKey>(TKey[] ids) where TEntity : class, IEntity<TKey>, new()
        {
            var entities = ids.Select(id => new TEntity {Id = id});
            RemoveRange(entities);
            try
            {
                await SaveChangesAsync();
            }
            catch (ForbiddenException)
            {
                throw;
            }
            catch (Exception)
            {
                var message = entities.Any(e => e is IActivatable)
                    ? "Cannot delete, you could set entities to inactive if you want."
                    : "Cannot delete.";
                throw new EntityReferenceException(message);
            }
        }

        public async Task UpdateNestedCollectionAsync<TEntity>(IEnumerable<TEntity> entity,
            Expression<Func<TEntity, bool>> condition, bool saveChanges = false)
            where TEntity : class, IEntity<Guid>, new()
        {
            var entities = await Set<TEntity>()
                .Where(condition)
                .ToListAsync();
            var ids = entity.Select(p => p.Id).ToList();
            var removedEntities = entities.Where(p => !ids.Contains(p.Id)).ToList();
            if (removedEntities.Any())
            {
                Set<TEntity>().RemoveRange(removedEntities);
            }

            var addedEntities = entity.Where(p => p.Id == Guid.Empty).ToList();
            if (addedEntities.Any())
            {
                await Set<TEntity>().AddRangeAsync(addedEntities);
            }

            var updatedEntities = entity.Except(addedEntities).ToList();
            if (updatedEntities.Any())
            {
                Set<TEntity>().UpdateRange(updatedEntities);
            }

            if (saveChanges)
            {
                await SaveChangesAsync();
            }
        }

        public void RemoveRange<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            Set<TEntity>().RemoveRange(entities);
        }

        public override int SaveChanges()
        {
            throw new Exception("Do not use SaveChanges, use SaveChangesAsync instead");
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.DetectChanges();

            BeforeSaveTriggers();

            // avoid calling DetectChanges again
            ChangeTracker.AutoDetectChangesEnabled = false;
            var result = await SaveChangesWithExceptionHandlingAsync(cancellationToken);
            ChangeTracker.AutoDetectChangesEnabled = true;

            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.DetectChanges();

            BeforeSaveTriggers();

            // avoid calling DetectChanges again
            ChangeTracker.AutoDetectChangesEnabled = false;
            var result = await SaveChangesWithExceptionHandlingAsync(cancellationToken, acceptAllChangesOnSuccess);
            ChangeTracker.AutoDetectChangesEnabled = true;

            return result;
        }

        private async Task<int> SaveChangesWithExceptionHandlingAsync(CancellationToken cancellationToken,
            bool acceptAllChangesOnSuccess = true)
        {
            try
            {
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            }
            //TODO add a decoder
            catch (DbUpdateException)
            {
                throw;
            }
        }

        private void BeforeSaveTriggers()
        {
            var changedEntities = ChangeTracker.Entries()
                .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));
            var utcNow = DateTime.UtcNow;

            foreach (var changedEntity in changedEntities)
            {
                // for entities that inherit from IMeta,
                // set UpdatedOn / CreatedOn appropriately
                if (changedEntity.Entity is IMeta entity)
                {
                    switch (changedEntity .State)
                    {
                        case EntityState.Modified:
                            // set the updated date to "now"
                            entity.UpdatedOn = utcNow;

                            // mark property as not modified (if done by mistake)
                            // we don't want to update on a Modify operation
                            changedEntity.Property("CreatedOn").IsModified = false;
                            break;

                        case EntityState.Added:
                            // set both updated and created date to "now"
                            entity.CreatedOn = utcNow;
                            entity.UpdatedOn = null;
                            break;
                    }
                }
            }
        }
    }
}