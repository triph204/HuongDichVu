namespace Order.Domain.ValueObjects
{
    /// <summary>
    /// Value Object - Order Number (Mã ??n hàng)
    /// ??m b?o tính immutability và validation
    /// </summary>
    public sealed class OrderNumber : IEquatable<OrderNumber>
    {
        public string Value { get; }

        private OrderNumber(string value)
        {
            Value = value;
        }

        public static OrderNumber Create()
        {
            return new OrderNumber($"ORD-{DateTime.Now:yyMMddHHmmss}");
        }

        public static OrderNumber CreateFrom(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Order number cannot be empty", nameof(value));

            if (!value.StartsWith("ORD-"))
                throw new ArgumentException("Order number must start with 'ORD-'", nameof(value));

            return new OrderNumber(value);
        }

        public bool Equals(OrderNumber? other)
        {
            if (other is null) return false;
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as OrderNumber);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString() => Value;

        public static implicit operator string(OrderNumber orderNumber) => orderNumber.Value;
    }
}
