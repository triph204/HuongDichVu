using System.ComponentModel.DataAnnotations;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO c?p nh?t ??n hàng - V?i security validation
    /// </summary>
    public class UpdateOrderDto : IValidatableObject
    {
        [StringLength(500, ErrorMessage = "Ghi chú không ???c v??t quá 500 ký t?")]
        public string? CustomerNote { get; set; }

        [Range(0, 999999999.99, ErrorMessage = "T?ng ti?n ph?i t? 0 ??n 999,999,999.99")]
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Custom validation logic
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Validate CustomerNote không ch?a script/XSS
            if (!string.IsNullOrEmpty(CustomerNote))
            {
                if (CustomerNote.Contains("<script", StringComparison.OrdinalIgnoreCase) ||
                    CustomerNote.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
                    CustomerNote.Contains("onclick", StringComparison.OrdinalIgnoreCase) ||
                    CustomerNote.Contains("onerror", StringComparison.OrdinalIgnoreCase) ||
                    CustomerNote.Contains("onload", StringComparison.OrdinalIgnoreCase))
                {
                    yield return new ValidationResult(
                        "Ghi chú ch?a n?i dung không h?p l?",
                        new[] { nameof(CustomerNote) });
                }
            }
        }
    }
}
