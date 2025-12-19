using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// Order Repository - K? th?a t? Base Repository
    /// Ch? implement các query ??c bi?t cho Order
    /// </summary>
    public class OrderRepository : Repository<OrderEntity>, IOrderRepository
    {
        public OrderRepository(OrderDbContext context) : base(context)
        {
        }

        // Override ?? include OrderDetails
        public override async Task<IEnumerable<OrderEntity>> GetAllAsync()
        {
            return await DbSet
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        // Override ?? include OrderDetails
        public override async Task<OrderEntity?> GetByIdAsync(int id)
        {
            return await DbSet
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<OrderEntity>> GetByStatusAsync(string status)
        {
            return await DbSet
                .Include(o => o.OrderDetails)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderEntity>> GetByTableIdAsync(int tableId)
        {
            return await DbSet
                .Include(o => o.OrderDetails)
                .Where(o => o.TableId == tableId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
    }
}
