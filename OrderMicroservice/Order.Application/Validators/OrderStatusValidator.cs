using Order.Domain.Constants;

namespace Order.Application.Validators
{
    /// <summary>
    /// Validator cho Order Status
    /// ??m b?o ch? accept các tr?ng thái h?p l?
    /// </summary>
    public static class OrderStatusValidator
    {
        private static readonly HashSet<string> ValidStatuses = new()
        {
            OrderStatus.PendingConfirmation,
            OrderStatus.Confirmed,
            OrderStatus.Cooking,
            OrderStatus.Completed,
            OrderStatus.Cancelled
        };

        public static bool IsValid(string status)
        {
            return !string.IsNullOrWhiteSpace(status) && ValidStatuses.Contains(status);
        }

        public static (bool IsValid, string Error) Validate(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return (false, "Tr?ng thái không ???c ?? tr?ng");

            if (!ValidStatuses.Contains(status))
                return (false, $"Tr?ng thái '{status}' không h?p l?. Các tr?ng thái h?p l?: {string.Join(", ", ValidStatuses)}");

            return (true, string.Empty);
        }

        public static IEnumerable<string> GetAllValidStatuses()
        {
            return ValidStatuses;
        }
    }
}
