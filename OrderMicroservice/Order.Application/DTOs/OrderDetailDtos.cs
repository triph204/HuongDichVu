namespace Order.Application.DTOs
{
    /// <summary>
    /// DTO cho chi ti?t ??n hàng
    /// </summary>
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? DishNote { get; set; }
    }

    /// <summary>
    /// DTO ?? thêm chi ti?t vào ??n hàng
    /// </summary>
    public class AddOrderDetailDto
    {
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? DishNote { get; set; }
    }

    /// <summary>
    /// DTO ?? t?o chi ti?t ??n hàng
    /// </summary>
    public class CreateOrderDetailDto
    {
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? DishNote { get; set; }
    }

    /// <summary>
    /// DTO ?? c?p nh?t chi ti?t ??n hàng
    /// </summary>
    public class UpdateOrderDetailDto
    {
        public int Quantity { get; set; }
    }
}
