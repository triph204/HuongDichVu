namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO ?? l?y d? li?u ??n hàng
    /// </summary>
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? CustomerNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public ICollection<OrderDetailDto> Details { get; set; } = new List<OrderDetailDto>();
    }

    /// <summary>
    /// DTO ?? t?o ??n hàng m?i
    /// </summary>
    public class CreateOrderDto
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string? CustomerNote { get; set; }
        public List<CreateOrderDetailDto> Items { get; set; } = new();
    }

    /// <summary>
    /// DTO ?? c?p nh?t ??n hàng
    /// </summary>
    public class UpdateOrderDto
    {
        public string? CustomerNote { get; set; }
        public decimal? TotalAmount { get; set; }
    }

    /// <summary>
    /// DTO ?? c?p nh?t tr?ng thái ??n hàng
    /// </summary>
    public class UpdateOrderStatusDto
    {
        public string NewStatus { get; set; } = string.Empty;
    }
}
