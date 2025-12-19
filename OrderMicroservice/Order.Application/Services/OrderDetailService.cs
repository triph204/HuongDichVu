using Order.Application.DTOs;
using Order.Application.Mappers;
using Order.Domain.Entities;
using Order.Infrastructure.Repositories;

namespace Order.Application.Services
{
    /// <summary>
    /// Order Detail Service - Application Layer
    /// Áp d?ng SOLID principles và Clean Architecture
    /// </summary>
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _detailRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderDetailService(
            IOrderDetailRepository detailRepository, 
            IOrderRepository orderRepository)
        {
            _detailRepository = detailRepository ?? throw new ArgumentNullException(nameof(detailRepository));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        }

        public async Task<IEnumerable<OrderDetailDto>> GetDetailsByOrderIdAsync(int orderId)
        {
            var details = await _detailRepository.GetByOrderIdAsync(orderId);
            return OrderDetailMapper.ToDtoList(details);
        }

        public async Task<OrderDetailDto?> AddDishToOrderAsync(int orderId, AddOrderDetailDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) 
                return null;

            try
            {
                // S? d?ng Domain Logic ?? thêm món
                order.AddOrderDetail(
                    dto.DishId,
                    dto.DishName,
                    dto.Quantity,
                    dto.UnitPrice,
                    dto.DishNote
                );

                await _orderRepository.UpdateAsync(order);

                // L?y detail v?a thêm
                var addedDetail = order.OrderDetails.FirstOrDefault(d => d.DishId == dto.DishId);
                return addedDetail != null ? OrderDetailMapper.ToDto(addedDetail) : null;
            }
            catch (InvalidOperationException)
            {
                // Business rule violation (vd: ??n hàng ?ã hoàn thành)
                return null;
            }
        }

        public async Task<bool> UpdateQuantityAsync(int detailId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("S? l??ng ph?i l?n h?n 0", nameof(quantity));

            var detail = await _detailRepository.GetByIdAsync(detailId);
            if (detail == null) 
                return false;

            try
            {
                // S? d?ng Domain Logic ?? update quantity
                detail.UpdateQuantity(quantity);
                await _detailRepository.UpdateAsync(detail);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public async Task<bool> RemoveDishFromOrderAsync(int detailId)
        {
            var detail = await _detailRepository.GetByIdAsync(detailId);
            if (detail == null) 
                return false;

            await _detailRepository.DeleteAsync(detailId);
            return true;
        }
    }
}
