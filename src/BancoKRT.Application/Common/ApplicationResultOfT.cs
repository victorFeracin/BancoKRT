namespace BancoKRT.Application.Common
{
    public class ApplicationResult<T> : ApplicationResult
    {
        public T? Value { get; }

        private ApplicationResult(bool isSuccess, T? value, ApplicationError? error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static ApplicationResult<T> Success(T value)
        {
            return new ApplicationResult<T>(true, value, null);
        }

        public new static ApplicationResult<T> Failure(ApplicationErrorType type, string message)
        {
            return new ApplicationResult<T>(false, default, new ApplicationError(type, message));
        }
    }
}
