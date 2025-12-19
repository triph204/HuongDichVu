using Order.Application.DTOs;
using Order.Application.Security;
using Order.Domain.Entities;

namespace Order.Application.Mappers
{
    /// <summary>
    /// Mapper cho OrderDetail Entity <-> DTO
    /// Single Responsibility Principle: Ch? ??m nhi?m vi?c mapping
    /// Có sanitize d? li?u ?? phòng ch?ng XSS
    /// </summary>
    public static class OrderDetailMapper
    {
        /// <summary>
        /// Map t? OrderDetailEntity sang OrderDetailDto
        /// Sanitize d? li?u tr??c khi tr? v? client
        /// </summary>
        public static OrderDetailDto ToDto(OrderDetailEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new OrderDetailDto
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                DishId = entity.DishId,
                DishName = InputSanitizer.SanitizeDishName(entity.DishName),
                Quantity = entity.Quantity,
                UnitPrice = entity.UnitPrice,
                TotalPrice = entity.TotalPrice,
                DishNote = InputSanitizer.SanitizeString(entity.DishNote)
            };
        }

        /// <summary>
        /// Map collection t? OrderDetailEntity sang OrderDetailDto
        /// </summary>
        public static IEnumerable<OrderDetailDto> ToDtoList(IEnumerable<OrderDetailEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            return entities.Select(ToDto);
        }
    }
}
