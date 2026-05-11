namespace BancoKRT.Application.Common
{
    public sealed record ApplicationError(ApplicationErrorType Type, string Message);
}
