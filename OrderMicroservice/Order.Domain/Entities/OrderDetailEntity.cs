namespace Order.Domain.Entities
{
    /// <summary>
    /// Entity chi ti?t ??n hàng (t?ng món ?n trong ??n)
    /// Áp d?ng Domain-Driven Design principles
    /// </summary>
    public class OrderDetailEntity
    {
        // Properties - Private setter ?? ??m b?o encapsulation
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int DishId { get; private set; }
        public string DishName { get; private set; } = string.Empty;
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal TotalPrice { get; private set; }
        public string? DishNote { get; private set; }

        // Navigation property
        public OrderEntity Order { get; private set; } = null!;

        // Constructor for EF Core
        private OrderDetailEntity() { }

        // Static Factory Method - Single Responsibility Principle
        public static OrderDetailEntity Create(
            int dishId,
            string dishName,
            int quantity,
            decimal unitPrice,
            string? note = null)
        {
            if (dishId <= 0)
                throw new ArgumentException("DishId ph?i l?n h?n 0", nameof(dishId));

            if (string.IsNullOrWhiteSpace(dishName))
                throw new ArgumentException("Tên món không ???c ?? tr?ng", nameof(dishName));

            if (quantity <= 0)
                throw new ArgumentException("S? l??ng ph?i l?n h?n 0", nameof(quantity));

            if (unitPrice <= 0)
                throw new ArgumentException("Giá ph?i l?n h?n 0", nameof(unitPrice));

            return new OrderDetailEntity
            {
                DishId = dishId,
                DishName = dishName,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = quantity * unitPrice,
                DishNote = note
            };
        }

        // Domain Logic - C?p nh?t s? l??ng
        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("S? l??ng ph?i l?n h?n 0", nameof(newQuantity));

            Quantity = newQuantity;
            TotalPrice = Quantity * UnitPrice;
        }

        // Domain Logic - C?p nh?t ghi chú món ?n
        public void UpdateNote(string? note)
        {
            DishNote = note;
        }
    }
}
