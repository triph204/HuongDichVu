using Microsoft.EntityFrameworkCore.Storage;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work Pattern Implementation
    /// Qu?n lý repositories và transactions
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderDbContext _context;
        private IDbContextTransaction? _transaction;
        private IOrderRepository? _orders;
        private IOrderDetailRepository? _orderDetails;

        public UnitOfWork(OrderDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IOrderRepository Orders
        {
            get
            {
                _orders ??= new OrderRepository(_context);
                return _orders;
            }
        }

        public IOrderDetailRepository OrderDetails
        {
            get
            {
                _orderDetails ??= new OrderDetailRepository(_context);
                return _orderDetails;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                }
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
