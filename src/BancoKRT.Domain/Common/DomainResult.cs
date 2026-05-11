namespace BancoKRT.Domain.Common
{
    public class DomainResult
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public DomainError? Error { get; }

        protected DomainResult(bool isSuccess, DomainError? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static DomainResult Success()
        {
            return new DomainResult(true, null);
        }

        public static DomainResult Failure(DomainErrorType type, string message)
        {
            return new DomainResult(false, new DomainError(type, message));
        }
    }
}
