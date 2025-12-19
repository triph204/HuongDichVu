using Order.Domain.Entities;

namespace Order.Infrastructure.Repositories
{
    public interface IOrderDetailRepository
    {
        Task<IEnumerable<OrderDetailEntity>> GetByOrderIdAsync(int orderId);
        Task<OrderDetailEntity?> GetByIdAsync(int id);
        Task<OrderDetailEntity> CreateAsync(OrderDetailEntity detail);
        Task<bool> UpdateAsync(OrderDetailEntity detail);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
