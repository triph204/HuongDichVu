namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// Base Repository Interface
    /// Generic repository pattern v?i các operations c? b?n
    /// </summary>
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<IEnumerable<TEntity>> GetAllAsync();
        Task<TEntity?> GetByIdAsync(int id);
        Task<TEntity> CreateAsync(TEntity entity);
        Task<bool> UpdateAsync(TEntity entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
