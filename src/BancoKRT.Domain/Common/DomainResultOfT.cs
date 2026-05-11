namespace BancoKRT.Domain.Common
{
    public class DomainResult<T> : DomainResult
    {
        public T? Value { get; }

        private DomainResult(bool isSuccess, T? value, DomainError? error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static DomainResult<T> Success(T value)
        {
            return new DomainResult<T>(true, value, null);
        }

        public new static DomainResult<T> Failure(DomainErrorType type, string message)
        {
            return new DomainResult<T>(false, default, new DomainError(type, message));
        }
    }
}
