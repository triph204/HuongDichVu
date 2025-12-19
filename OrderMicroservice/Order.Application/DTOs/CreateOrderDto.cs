using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO t?o ??n hàng m?i - V?i security validation
    /// </summary>
    public class CreateOrderDto : IValidatableObject
    {
        [Required(ErrorMessage = "TableId là b?t bu?c")]
        [Range(1, int.MaxValue, ErrorMessage = "TableId ph?i l?n h?n 0")]
        public int TableId { get; set; }

        [Required(ErrorMessage = "Tên bàn là b?t bu?c")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Tên bàn ph?i t? 1-50 ký t?")]
        [RegularExpression(@"^[a-zA-Z0-9\s\u00C0-\u1EF9]+$", ErrorMessage = "Tên bàn ch? ???c ch?a ch?, s? và kho?ng tr?ng")]
        public string TableName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Ghi chú không ???c v??t quá 500 ký t?")]
        public string? CustomerNote { get; set; }

        [Required(ErrorMessage = "??n hàng ph?i có ít nh?t 1 món")]
        [MinLength(1, ErrorMessage = "??n hàng ph?i có ít nh?t 1 món")]
        [MaxLength(50, ErrorMessage = "??n hàng không ???c v??t quá 50 món")]
        public List<CreateOrderDetailDto> Items { get; set; } = new List<CreateOrderDetailDto>();

        /// <summary>
        /// Custom validation logic
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ki?m tra Items không null và có ít nh?t 1 món
            if (Items == null || Items.Count == 0)
            {
                yield return new ValidationResult(
                    "??n hàng ph?i có ít nh?t 1 món",
                    new[] { nameof(Items) });
            }

            // Ki?m tra không có duplicate DishId
            if (Items != null)
            {
                var duplicateDishIds = Items
                    .GroupBy(x => x.DishId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateDishIds.Any())
                {
                    yield return new ValidationResult(
                        $"Có món ?n b? trùng l?p: {string.Join(", ", duplicateDishIds)}",
                        new[] { nameof(Items) });
                }

                // Validate t?ng s? l??ng món không quá l?n (DoS prevention)
                var totalQuantity = Items.Sum(x => x.Quantity);
                if (totalQuantity > 1000)
                {
                    yield return new ValidationResult(
                        "T?ng s? l??ng món không ???c v??t quá 1000",
                        new[] { nameof(Items) });
                }
            }

            // Validate CustomerNote không ch?a script
            if (!string.IsNullOrEmpty(CustomerNote))
            {
                if (CustomerNote.Contains("<script", StringComparison.OrdinalIgnoreCase) ||
                    CustomerNote.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
                    CustomerNote.Contains("onclick", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        "Ghi chú ch?a n?i dung không h?p l?",
                        new[] { nameof(CustomerNote) });
                }
            }
        }
    }
}
