using Order.Domain.Entities;

namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// Order Repository Interface - K? th?a t? Base Repository
    /// Interface Segregation Principle (ISP)
    /// </summary>
    public interface IOrderRepository : IRepository<OrderEntity>
    {
        Task<IEnumerable<OrderEntity>> GetByStatusAsync(string status);
        Task<IEnumerable<OrderEntity>> GetByTableIdAsync(int tableId);
    }
}
