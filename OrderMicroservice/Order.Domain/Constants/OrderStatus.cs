namespace Order.Domain.Constants
{
    /// <summary>
    /// Tr?ng thái ??n hàng
    /// </summary>
    public static class OrderStatus
    {
        public const string PendingConfirmation = "ChoXacNhan"; // Ch? xác nh?n
        public const string Confirmed = "DaXacNhan"; // ?ã xác nh?n
        public const string Cooking = "DangChuan"; // ?ang chu?n b?
        public const string Completed = "HoanThanh"; // Hoàn thành
        public const string Cancelled = "Huy"; // H?y
    }
}
