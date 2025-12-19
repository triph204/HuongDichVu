using Order.Domain.Entities;

namespace Order.Infrastructure.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderEntity>> GetAllAsync();
        Task<OrderEntity?> GetByIdAsync(int id);
        Task<IEnumerable<OrderEntity>> GetByStatusAsync(string status);
        Task<IEnumerable<OrderEntity>> GetByTableIdAsync(int tableId);
        Task<OrderEntity> CreateAsync(OrderEntity order);
        Task<bool> UpdateAsync(OrderEntity order);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
