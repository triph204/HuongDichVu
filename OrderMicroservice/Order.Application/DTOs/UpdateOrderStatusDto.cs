using System.ComponentModel.DataAnnotations;
using Order.Domain.Constants;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO c?p nh?t tr?ng thái ??n hàng - V?i security validation
    /// </summary>
    public class UpdateOrderStatusDto : IValidatableObject
    {
        [Required(ErrorMessage = "Tr?ng thái m?i là b?t bu?c")]
        [StringLength(50, ErrorMessage = "Tr?ng thái không ???c v??t quá 50 ký t?")]
        public string NewStatus { get; set; } = string.Empty;

        // Danh sách các tr?ng thái h?p l?
        private static readonly HashSet<string> ValidStatuses = new()
        {
            OrderStatus.PendingConfirmation,
            OrderStatus.Confirmed,
            OrderStatus.Cooking,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };

        /// <summary>
        /// Custom validation logic
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate status là m?t trong các giá tr? cho phép (whitelist approach)
            if (!ValidStatuses.Contains(NewStatus))
            {
                yield return new ValidationResult(
                    $"Tr?ng thái '{NewStatus}' không h?p l?. Các tr?ng thái h?p l?: {string.Join(", ", ValidStatuses)}",
                    new[] { nameof(NewStatus) });
            }

            // Phòng ch?ng injection qua status
            if (!string.IsNullOrEmpty(NewStatus))
            {
                if (NewStatus.Contains("<") || 
                    NewStatus.Contains(">") || 
                    NewStatus.Contains("'") ||
                    NewStatus.Contains("\"") ||
                    NewStatus.Contains(";") ||
                    NewStatus.Contains("--"))
                {
                    yield return new ValidationResult(
                        "Tr?ng thái ch?a ký t? không h?p l?",
                        new[] { nameof(NewStatus) });
                }
            }
        }
    }
}
