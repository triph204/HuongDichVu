using Microsoft.EntityFrameworkCore;
using Order.Domain.Entities;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly OrderDbContext _context;

        public OrderDetailRepository(OrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDetailEntity>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderDetails
                .Where(d => d.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderDetailEntity?> GetByIdAsync(int id)
        {
            return await _context.OrderDetails.FindAsync(id);
        }

        public async Task<OrderDetailEntity> CreateAsync(OrderDetailEntity detail)
        {
            _context.OrderDetails.Add(detail);
            await _context.SaveChangesAsync();
            return detail;
        }

        public async Task<bool> UpdateAsync(OrderDetailEntity detail)
        {
            _context.OrderDetails.Update(detail);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var detail = await _context.OrderDetails.FindAsync(id);
            if (detail == null) return false;

            _context.OrderDetails.Remove(detail);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.OrderDetails.AnyAsync(d => d.Id == id);
        }
    }
}
