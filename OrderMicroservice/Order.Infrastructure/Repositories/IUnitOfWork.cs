namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work Pattern Interface
    /// Qu?n lý transactions và ??m b?o tính nh?t quán c?a d? li?u
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IOrderRepository Orders { get; }
        IOrderDetailRepository OrderDetails { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}
