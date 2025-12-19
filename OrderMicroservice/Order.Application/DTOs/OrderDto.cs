using System.ComponentModel.DataAnnotations;

namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO tr? v? thông tin ??n hàng ??y ?? (Response DTO)
    /// ?ã sanitize d? li?u tr??c khi tr? v? client
    /// </summary>
    public class OrderDto
    {
        public int Id { get; set; }
        
        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;
        
        public int TableId { get; set; }
        
        [MaxLength(50)]
        public string TableName { get; set; } = string.Empty;
        
        [Range(0, 999999999.99)]
        public decimal TotalAmount { get; set; }
        
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? CustomerNote { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public ICollection<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();
    }
}
