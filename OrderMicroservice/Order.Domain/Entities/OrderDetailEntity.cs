namespace Order.Domain.Entities
{
    /// <summary>
    /// Entity chi ti?t ??n hàng (t?ng món ?n trong ??n)
    /// </summary>
    public class OrderDetailEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? DishNote { get; set; }

        // Relationships
        public OrderEntity Order { get; set; } = null!;
    }
}
