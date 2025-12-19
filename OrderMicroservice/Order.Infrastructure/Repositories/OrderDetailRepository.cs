using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// OrderDetail Repository - K? th?a t? Base Repository
    /// Ch? implement các query ??c bi?t cho OrderDetail
    /// </summary>
    public class OrderDetailRepository : Repository<OrderDetailEntity>, IOrderDetailRepository
    {
        public OrderDetailRepository(OrderDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<OrderDetailEntity>> GetByOrderIdAsync(int orderId)
        {
            return await DbSet
                .Where(d => d.OrderId == orderId)
                .ToListAsync();
        }
    }
}
