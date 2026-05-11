namespace BancoKRT.Domain.Common
{
    public sealed record DomainError(DomainErrorType Type, string Message);
}
