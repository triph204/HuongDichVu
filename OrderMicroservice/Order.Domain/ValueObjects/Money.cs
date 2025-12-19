namespace Order.Domain.ValueObjects
{
    /// <summary>
    /// Value Object - Money (Ti?n t?)
    /// ??m b?o tính toán chính xác và validation cho s? ti?n
    /// </summary>
    public sealed class Money : IEquatable<Money>, IComparable<Money>
    {
        public decimal Amount { get; }

        private Money(decimal amount)
        {
            Amount = amount;
        }

        public static Money Create(decimal amount)
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            return new Money(amount);
        }

        public static Money Zero => new Money(0);

        public Money Add(Money other)
        {
            return new Money(Amount + other.Amount);
        }

        public Money Subtract(Money other)
        {
            if (Amount < other.Amount)
                throw new InvalidOperationException("Cannot subtract to negative amount");

            return new Money(Amount - other.Amount);
        }

        public Money Multiply(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            return new Money(Amount * quantity);
        }

        public bool Equals(Money? other)
        {
            if (other is null) return false;
            return Amount == other.Amount;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Money);
        }

        public override int GetHashCode()
        {
            return Amount.GetHashCode();
        }

        public int CompareTo(Money? other)
        {
            if (other is null) return 1;
            return Amount.CompareTo(other.Amount);
        }

        public override string ToString() => Amount.ToString("N0");

        public static implicit operator decimal(Money money) => money.Amount;
        
        public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
        public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
        public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;
        public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
    }
}
