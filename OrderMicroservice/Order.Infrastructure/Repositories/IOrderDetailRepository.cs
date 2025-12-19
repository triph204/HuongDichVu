using Order.Domain.Entities;

namespace Order.Infrastructure.Repositories
{
    /// <summary>
    /// OrderDetail Repository Interface - K? th?a t? Base Repository
    /// Interface Segregation Principle (ISP)
    /// </summary>
    public interface IOrderDetailRepository : IRepository<OrderDetailEntity>
    {
        Task<IEnumerable<OrderDetailEntity>> GetByOrderIdAsync(int orderId);
    }
}
