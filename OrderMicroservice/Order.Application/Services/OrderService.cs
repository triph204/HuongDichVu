using Order.Application.Common;
using Order.Application.DTOs;
using Order.Application.Mappers;
using Order.Domain.Entities;
using Order.Infrastructure.Repositories;

namespace Order.Application.Services
{
    /// <summary>
    /// Order Service - Application Layer
    /// Áp d?ng SOLID principles và Clean Architecture
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;

        public OrderService(
            IOrderRepository orderRepository, 
            IOrderDetailRepository orderDetailRepository)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _orderDetailRepository = orderDetailRepository ?? throw new ArgumentNullException(nameof(orderDetailRepository));
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return OrderMapper.ToDtoList(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : OrderMapper.ToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return OrderMapper.ToDtoList(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByTableIdAsync(int tableId)
        {
            var orders = await _orderRepository.GetByTableIdAsync(tableId);
            return OrderMapper.ToDtoList(orders);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Items == null || !dto.Items.Any())
                throw new ArgumentException("??n hàng ph?i có ít nh?t 1 món", nameof(dto.Items));

            // S? d?ng Factory Method t? Domain Entity
            var order = OrderMapper.ToEntity(dto);

            // L?u vào database
            var created = await _orderRepository.CreateAsync(order);

            return OrderMapper.ToDto(created);
        }

        public async Task<bool> UpdateOrderAsync(int id, UpdateOrderDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) 
                return false;

            // S? d?ng Domain Logic ?? update
            if (!string.IsNullOrEmpty(dto.CustomerNote))
            {
                order.UpdateCustomerNote(dto.CustomerNote);
            }

            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Tr?ng thái m?i không ???c ?? tr?ng", nameof(newStatus));

            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) 
                return false;

            try
            {
                // S? d?ng Domain Logic ?? validate và update status
                order.UpdateStatus(newStatus);
                await _orderRepository.UpdateAsync(order);
                return true;
            }
            catch (InvalidOperationException)
            {
                // Business rule violation
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            return await _orderRepository.DeleteAsync(id);
        }
    }
}
