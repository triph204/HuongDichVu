using Order.Application.DTOs;
using Order.Domain.Constants;

namespace Order.Application.Validators
{
    /// <summary>
    /// Validator cho CreateOrderDto
    /// Single Responsibility: Ch? validate business rules
    /// </summary>
    public static class CreateOrderDtoValidator
    {
        public static (bool IsValid, List<string> Errors) Validate(CreateOrderDto dto)
        {
            var errors = new List<string>();

            if (dto == null)
            {
                errors.Add("DTO không ???c null");
                return (false, errors);
            }

            if (dto.TableId <= 0)
                errors.Add("TableId ph?i l?n h?n 0");

            if (string.IsNullOrWhiteSpace(dto.TableName))
                errors.Add("Tên bàn không ???c ?? tr?ng");

            if (dto.Items == null || !dto.Items.Any())
                errors.Add("??n hàng ph?i có ít nh?t 1 món");

            if (dto.Items != null)
            {
                for (int i = 0; i < dto.Items.Count; i++)
                {
                    var item = dto.Items[i];
                    
                    if (item.DishId <= 0)
                        errors.Add($"Món th? {i + 1}: DishId ph?i l?n h?n 0");

                    if (string.IsNullOrWhiteSpace(item.DishName))
                        errors.Add($"Món th? {i + 1}: Tên món không ???c ?? tr?ng");

                    if (item.Quantity <= 0)
                        errors.Add($"Món th? {i + 1}: S? l??ng ph?i l?n h?n 0");

                    if (item.UnitPrice <= 0)
                        errors.Add($"Món th? {i + 1}: Giá ph?i l?n h?n 0");
                }
            }

            return (errors.Count == 0, errors);
        }
    }
}
