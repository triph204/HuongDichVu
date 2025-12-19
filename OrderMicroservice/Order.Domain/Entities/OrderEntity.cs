namespace Order.Domain.Entities
{
    /// <summary>
    /// Entity ??n hàng
    /// </summary>
    public class OrderEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } // Mã s? ??n (ORD-yyMMddHHmmss)
        public int TableId { get; set; }
        public string TableName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // ChoXacNhan, DaXacNhan, DangChuan, HoanThanh, Huy
        public string? CustomerNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Relationships
        public ICollection<OrderDetailEntity> OrderDetails { get; set; } = new List<OrderDetailEntity>();
    }
}
