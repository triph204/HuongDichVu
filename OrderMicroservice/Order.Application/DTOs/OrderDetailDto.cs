using System.ComponentModel.DataAnnotations;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO tr? v? thông tin chi ti?t ??n hàng (món ?n trong ??n)
    /// </summary>
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DishId { get; set; }
        
        [MaxLength(200)]
        public string DishName { get; set; } = string.Empty;
        
        [Range(1, 1000)]
        public int Quantity { get; set; }
        
        [Range(0, 999999999.99)]
        public decimal UnitPrice { get; set; }
        
        [Range(0, 999999999.99)]
        public decimal TotalPrice { get; set; }
        
        [MaxLength(300)]
        public string? DishNote { get; set; }
    }
}
