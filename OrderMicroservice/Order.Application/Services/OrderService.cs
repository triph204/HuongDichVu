using Order.Application.DTOs;
using Order.Domain.Entities;
using Order.Infrastructure.Repositories;

namespace Order.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        public OrderService(IOrderRepository orderRepository, IOrderDetailRepository orderDetailRepository)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByTableIdAsync(int tableId)
        {
            var orders = await _orderRepository.GetByTableIdAsync(tableId);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new OrderEntity
            {
                OrderNumber = $"ORD-{DateTime.Now:yyMMddHHmmss}",
                TableId = dto.TableId,
                TableName = dto.TableName,
                Status = "ChoXacNhan",
                CustomerNote = dto.CustomerNote,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            decimal total = 0;
            foreach (var item in dto.Items)
            {
                var detail = new OrderDetailEntity
                {
                    DishId = item.DishId,
                    DishName = item.DishName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice,
                    DishNote = item.DishNote
                };
                order.OrderDetails.Add(detail);
                total += detail.TotalPrice;
            }

            order.TotalAmount = total;
            var created = await _orderRepository.CreateAsync(order);

            return MapToDto(created);
        }

        public async Task<bool> UpdateOrderAsync(int id, UpdateOrderDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            if (!string.IsNullOrEmpty(dto.CustomerNote))
                order.CustomerNote = dto.CustomerNote;

            if (dto.TotalAmount.HasValue)
                order.TotalAmount = dto.TotalAmount.Value;

            order.UpdatedAt = DateTime.Now;
            await _orderRepository.UpdateAsync(order);

            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

            order.Status = newStatus;
            order.UpdatedAt = DateTime.Now;

            if (newStatus == "HoanThanh")
                order.CompletedAt = DateTime.Now;

            await _orderRepository.UpdateAsync(order);

            return true;
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            return await _orderRepository.DeleteAsync(id);
        }

        // Helper method
        private OrderDto MapToDto(OrderEntity order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TableId = order.TableId,
                TableName = order.TableName,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                CustomerNote = order.CustomerNote,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                CompletedAt = order.CompletedAt,
                Details = order.OrderDetails.Select(d => new OrderDetailDto
                {
                    Id = d.Id,
                    OrderId = d.OrderId,
                    DishId = d.DishId,
                    DishName = d.DishName,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TotalPrice = d.TotalPrice,
                    DishNote = d.DishNote
                }).ToList()
            };
        }
    }
}
