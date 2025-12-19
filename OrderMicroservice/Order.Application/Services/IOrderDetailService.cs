using Order.Application.DTOs;

namespace Order.Application.Services
{
    public interface IOrderDetailService
    {
        Task<IEnumerable<OrderDetailDto>> GetDetailsByOrderIdAsync(int orderId);
        Task<OrderDetailDto?> AddDishToOrderAsync(int orderId, AddOrderDetailDto dto);
        Task<bool> UpdateQuantityAsync(int detailId, int quantity);
        Task<bool> RemoveDishFromOrderAsync(int detailId);
    }
}
