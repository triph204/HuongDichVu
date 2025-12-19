using Order.Domain.Constants;

namespace Order.Domain.Entities
{
    /// <summary>
    /// Entity ??n hàng - Aggregate Root
    /// Áp d?ng Domain-Driven Design principles
    /// </summary>
    public class OrderEntity
    {
        // Properties - Private setter ?? ??m b?o encapsulation
        public int Id { get; private set; }
        public string OrderNumber { get; private set; } = string.Empty;
        public int TableId { get; private set; }
        public string TableName { get; private set; } = string.Empty;
        public decimal TotalAmount { get; private set; }
        public string Status { get; private set; } = OrderStatus.PendingConfirmation;
        public string? CustomerNote { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        // Navigation - Private setter ?? b?o v? collection
        private readonly List<OrderDetailEntity> _orderDetails = new();
        public IReadOnlyCollection<OrderDetailEntity> OrderDetails => _orderDetails.AsReadOnly();

        // Constructor for EF Core
        private OrderEntity() { }

        // Static Factory Method - Single Responsibility Principle
        public static OrderEntity Create(
            int tableId, 
            string tableName, 
            string? customerNote = null)
        {
            if (tableId <= 0)
                throw new ArgumentException("TableId ph?i l?n h?n 0", nameof(tableId));

            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Tên bàn không ???c ?? tr?ng", nameof(tableName));

            return new OrderEntity
            {
                OrderNumber = GenerateOrderNumber(),
                TableId = tableId,
                TableName = tableName,
                CustomerNote = customerNote,
                Status = OrderStatus.PendingConfirmation,
                TotalAmount = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        // Domain Logic - Thêm món ?n
        public void AddOrderDetail(int dishId, string dishName, int quantity, decimal unitPrice, string? note = null)
        {
            if (!CanModifyOrder())
                throw new InvalidOperationException($"Không th? thêm món khi ??n hàng ? tr?ng thái {Status}");

            if (dishId <= 0)
                throw new ArgumentException("DishId ph?i l?n h?n 0", nameof(dishId));

            if (string.IsNullOrWhiteSpace(dishName))
                throw new ArgumentException("Tên món không ???c ?? tr?ng", nameof(dishName));

            if (quantity <= 0)
                throw new ArgumentException("S? l??ng ph?i l?n h?n 0", nameof(quantity));

            if (unitPrice <= 0)
                throw new ArgumentException("Giá ph?i l?n h?n 0", nameof(unitPrice));

            var existingDetail = _orderDetails.FirstOrDefault(d => d.DishId == dishId);
            
            if (existingDetail != null)
            {
                existingDetail.UpdateQuantity(existingDetail.Quantity + quantity);
            }
            else
            {
                var detail = OrderDetailEntity.Create(dishId, dishName, quantity, unitPrice, note);
                _orderDetails.Add(detail);
            }

            RecalculateTotalAmount();
            UpdatedAt = DateTime.Now;
        }

        // Domain Logic - Xóa món ?n
        public void RemoveOrderDetail(int detailId)
        {
            if (!CanModifyOrder())
                throw new InvalidOperationException($"Không th? xóa món khi ??n hàng ? tr?ng thái {Status}");

            var detail = _orderDetails.FirstOrDefault(d => d.Id == detailId);
            if (detail == null)
                throw new InvalidOperationException($"Không tìm th?y món ?n v?i ID {detailId}");

            _orderDetails.Remove(detail);
            RecalculateTotalAmount();
            UpdatedAt = DateTime.Now;
        }

        // Domain Logic - C?p nh?t tr?ng thái
        public void UpdateStatus(string newStatus)
        {
            if (!IsValidStatusTransition(newStatus))
                throw new InvalidOperationException($"Không th? chuy?n t? tr?ng thái {Status} sang {newStatus}");

            Status = newStatus;
            UpdatedAt = DateTime.Now;

            if (newStatus == OrderStatus.Completed)
            {
                CompletedAt = DateTime.Now;
            }
        }

        // Domain Logic - C?p nh?t ghi chú
        public void UpdateCustomerNote(string? note)
        {
            CustomerNote = note;
            UpdatedAt = DateTime.Now;
        }

        // Business Rules - Ki?m tra có th? s?a ??n không
        private bool CanModifyOrder()
        {
            return Status == OrderStatus.PendingConfirmation || Status == OrderStatus.Confirmed;
        }

        // Business Rules - Ki?m tra chuy?n tr?ng thái h?p l?
        private bool IsValidStatusTransition(string newStatus)
        {
            return (Status, newStatus) switch
            {
                (OrderStatus.PendingConfirmation, OrderStatus.Confirmed) => true,
                (OrderStatus.PendingConfirmation, OrderStatus.Cancelled) => true,
                (OrderStatus.Confirmed, OrderStatus.Cooking) => true,
                (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
                (OrderStatus.Cooking, OrderStatus.Completed) => true,
                _ => false
            };
        }

        // Helper - Tính l?i t?ng ti?n
        private void RecalculateTotalAmount()
        {
            TotalAmount = _orderDetails.Sum(d => d.TotalPrice);
        }

        // Helper - T?o mã ??n hàng
        private static string GenerateOrderNumber()
        {
            return $"ORD-{DateTime.Now:yyMMddHHmmss}";
        }
    }
}
