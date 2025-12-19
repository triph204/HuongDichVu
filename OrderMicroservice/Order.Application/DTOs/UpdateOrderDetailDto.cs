using System.ComponentModel.DataAnnotations;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO c?p nh?t s? l??ng món ?n trong ??n hàng - Có validation
    /// </summary>
    public class UpdateOrderDetailDto
    {
        [Required(ErrorMessage = "S? l??ng là b?t bu?c")]
        [Range(1, 1000, ErrorMessage = "S? l??ng ph?i t? 1-1000")]
        public int Quantity { get; set; }
    }
}
