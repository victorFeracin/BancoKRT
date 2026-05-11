namespace BancoKRT.Application.Common
{
    public class ApplicationResult
    {
        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public ApplicationError? Error { get; }

        protected ApplicationResult(bool isSuccess, ApplicationError? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static ApplicationResult Success()
        {
            return new ApplicationResult(true, null);
        }

        public static ApplicationResult Failure(ApplicationErrorType type, string message)
        {
            return new ApplicationResult(false, new ApplicationError(type, message));
        }
    }
}
