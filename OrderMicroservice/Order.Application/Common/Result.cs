namespace Order.Application.Common
{
    /// <summary>
    /// Result Pattern - ??i di?n cho k?t qu? c?a m?t operation
    /// Giúp x? lý error m?t cách rõ ràng thay vì exception
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public bool IsFailure => !IsSuccess;

        protected Result(bool isSuccess, string errorMessage)
        {
            if (isSuccess && !string.IsNullOrEmpty(errorMessage))
                throw new InvalidOperationException("Success result cannot have error message");

            if (!isSuccess && string.IsNullOrEmpty(errorMessage))
                throw new InvalidOperationException("Failure result must have error message");

            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static Result Success() => new Result(true, string.Empty);
        public static Result Failure(string error) => new Result(false, error);
        public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);
        public static Result<T> Failure<T>(string error) => new Result<T>(default!, false, error);
    }

    /// <summary>
    /// Generic Result Pattern with value
    /// </summary>
    public class Result<T> : Result
    {
        public T Value { get; }

        protected internal Result(T value, bool isSuccess, string errorMessage)
            : base(isSuccess, errorMessage)
        {
            Value = value;
        }
    }
}
