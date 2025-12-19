using Order.Application.DTOs;
using Order.Application.Security;
using Order.Domain.Entities;

namespace Order.Application.Mappers
{
    /// <summary>
    /// Mapper cho Order Entity <-> DTO
    /// Single Responsibility Principle: Ch? ??m nhi?m vi?c mapping
    /// Có sanitize d? li?u ?? phòng ch?ng XSS
    /// </summary>
    public static class OrderMapper
    {
        /// <summary>
        /// Map t? OrderEntity sang OrderDto
        /// Sanitize d? li?u tr??c khi tr? v? client
        /// </summary>
        public static OrderDto ToDto(OrderEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new OrderDto
            {
                Id = entity.Id,
                OrderNumber = InputSanitizer.SanitizeOrderNumber(entity.OrderNumber),
                TableId = entity.TableId,
                TableName = InputSanitizer.SanitizeTableName(entity.TableName),
                TotalAmount = entity.TotalAmount,
                Status = entity.Status, // Status ?ã ???c validate t? domain
                CustomerNote = InputSanitizer.SanitizeString(entity.CustomerNote),
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
                CompletedAt = entity.CompletedAt,
                Details = entity.OrderDetails.Select(OrderDetailMapper.ToDto).ToList()
            };
        }

        /// <summary>
        /// Map t? CreateOrderDto sang OrderEntity
        /// Sanitize input tr??c khi t?o entity
        /// </summary>
        public static OrderEntity ToEntity(CreateOrderDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Sanitize input tr??c khi t?o entity
            var sanitizedTableName = InputSanitizer.SanitizeTableName(dto.TableName);
            var sanitizedNote = InputSanitizer.SanitizeString(dto.CustomerNote);

            var order = OrderEntity.Create(
                dto.TableId,
                sanitizedTableName,
                sanitizedNote
            );

            foreach (var item in dto.Items)
            {
                var sanitizedDishName = InputSanitizer.SanitizeDishName(item.DishName);
                var sanitizedDishNote = InputSanitizer.SanitizeString(item.DishNote);

                order.AddOrderDetail(
                    item.DishId,
                    sanitizedDishName,
                    item.Quantity,
                    item.UnitPrice,
                    sanitizedDishNote
                );
            }

            return order;
        }

        /// <summary>
        /// Map collection t? OrderEntity sang OrderDto
        /// </summary>
        public static IEnumerable<OrderDto> ToDtoList(IEnumerable<OrderEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            return entities.Select(ToDto);
        }
    }
}
