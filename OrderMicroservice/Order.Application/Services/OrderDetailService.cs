using Order.Application.DTOs;
using Order.Domain.Entities;
using Order.Infrastructure.Repositories;

namespace Order.Application.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _detailRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderDetailService(IOrderDetailRepository detailRepository, IOrderRepository orderRepository)
        {
            _detailRepository = detailRepository;
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderDetailDto>> GetDetailsByOrderIdAsync(int orderId)
        {
            var details = await _detailRepository.GetByOrderIdAsync(orderId);
            return details.Select(MapToDto).ToList();
        }

        public async Task<OrderDetailDto?> AddDishToOrderAsync(int orderId, AddOrderDetailDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return null;

            // Check if dish already exists
            var existing = order.OrderDetails.FirstOrDefault(d => d.DishId == dto.DishId);
            if (existing != null)
            {
                // Update quantity if exists
                existing.Quantity += dto.Quantity;
                existing.TotalPrice = existing.Quantity * existing.UnitPrice;
                await _detailRepository.UpdateAsync(existing);
            }
            else
            {
                // Create new detail
                var detail = new OrderDetailEntity
                {
                    OrderId = orderId,
                    DishId = dto.DishId,
                    DishName = dto.DishName,
                    Quantity = dto.Quantity,
                    UnitPrice = dto.UnitPrice,
                    TotalPrice = dto.Quantity * dto.UnitPrice,
                    DishNote = dto.DishNote
                };
                await _detailRepository.CreateAsync(detail);
            }

            // Update order total
            order.TotalAmount = order.OrderDetails.Sum(d => d.TotalPrice);
            order.UpdatedAt = DateTime.Now;
            await _orderRepository.UpdateAsync(order);

            var updatedDetail = await _detailRepository.GetByIdAsync(existing?.Id ?? order.OrderDetails.Last().Id);
            return updatedDetail == null ? null : MapToDto(updatedDetail);
        }

        public async Task<bool> UpdateQuantityAsync(int detailId, int quantity)
        {
            var detail = await _detailRepository.GetByIdAsync(detailId);
            if (detail == null) return false;

            detail.Quantity = quantity;
            detail.TotalPrice = quantity * detail.UnitPrice;
            await _detailRepository.UpdateAsync(detail);

            return true;
        }

        public async Task<bool> RemoveDishFromOrderAsync(int detailId)
        {
            var detail = await _detailRepository.GetByIdAsync(detailId);
            if (detail == null) return false;

            await _detailRepository.DeleteAsync(detailId);
            return true;
        }

        private OrderDetailDto MapToDto(OrderDetailEntity detail)
        {
            return new OrderDetailDto
            {
                Id = detail.Id,
                OrderId = detail.OrderId,
                DishId = detail.DishId,
                DishName = detail.DishName,
                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                TotalPrice = detail.TotalPrice,
                DishNote = detail.DishNote
            };
        }
    }
}
