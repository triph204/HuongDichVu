using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// Base Repository Implementation
    /// Generic repository pattern implementation - DRY Principle
    /// </summary>
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly OrderDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(OrderDbContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            DbSet = context.Set<TEntity>();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await DbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await DbSet.FindAsync(id);
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await DbSet.AddAsync(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<bool> UpdateAsync(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            DbSet.Update(entity);
            await Context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> DeleteAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity == null) return false;

            DbSet.Remove(entity);
            await Context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            return entity != null;
        }
    }
}
