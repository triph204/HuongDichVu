using Order.Application.DTOs;

namespace Order.Application.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status);
        Task<IEnumerable<OrderDto>> GetOrdersByTableIdAsync(int tableId);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
        Task<bool> UpdateOrderAsync(int id, UpdateOrderDto dto);
        Task<bool> UpdateOrderStatusAsync(int id, string newStatus);
        Task<bool> DeleteOrderAsync(int id);
    }
}
