using System.ComponentModel.DataAnnotations;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO thêm món ?n vào ??n hàng ?ã t?n t?i - V?i security validation
    /// </summary>
    public class AddOrderDetailDto : IValidatableObject
    {
        [Required(ErrorMessage = "DishId là b?t bu?c")]
        [Range(1, int.MaxValue, ErrorMessage = "DishId ph?i l?n h?n 0")]
        public int DishId { get; set; }

        [Required(ErrorMessage = "Tên món ?n là b?t bu?c")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Tên món ph?i t? 1-200 ký t?")]
        public string DishName { get; set; } = string.Empty;

        [Range(1, 1000, ErrorMessage = "S? l??ng ph?i t? 1-1000")]
        public int Quantity { get; set; }

        [Range(0.01, 999999999.99, ErrorMessage = "Giá ph?i t? 0.01 ??n 999,999,999.99")]
        public decimal UnitPrice { get; set; }

        [StringLength(300, ErrorMessage = "Ghi chú món không ???c v??t quá 300 ký t?")]
        public string? DishNote { get; set; }

        /// <summary>
        /// Custom validation logic - Phòng ch?ng XSS và injection
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            // Validate DishName không ch?a script
            if (!string.IsNullOrEmpty(DishName) && ContainsDangerousContent(DishName))
            {
                results.Add(new ValidationResult(
                    "Tên món ch?a n?i dung không h?p l?",
                    new[] { nameof(DishName) }));
            }

            // Validate DishNote không ch?a script
            if (!string.IsNullOrEmpty(DishNote) && ContainsDangerousContent(DishNote))
            {
                results.Add(new ValidationResult(
                    "Ghi chú món ch?a n?i dung không h?p l?",
                    new[] { nameof(DishNote) }));
            }

            // Validate t?ng ti?n không overflow
            if (!IsValidTotalAmount())
            {
                results.Add(new ValidationResult(
                    "T?ng ti?n v??t quá gi?i h?n cho phép",
                    new[] { nameof(Quantity), nameof(UnitPrice) }));
            }

            return results;
        }

        /// <summary>
        /// Ki?m tra t?ng ti?n không overflow
        /// </summary>
        private bool IsValidTotalAmount()
        {
            try
            {
                var total = checked(Quantity * UnitPrice);
                return total <= 999999999.99m;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        /// <summary>
        /// Ki?m tra n?i dung nguy hi?m (XSS, injection)
        /// </summary>
        private static bool ContainsDangerousContent(string content)
        {
            var dangerousPatterns = new[]
            {
                "<script", "</script>", "javascript:", "onclick", "onerror", "onload",
                "onmouseover", "onfocus", "onblur", "<iframe", "<object", "<embed",
                "expression(", "vbscript:", "data:"
            };

            return dangerousPatterns.Any(pattern => 
                content.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }
    }
}
