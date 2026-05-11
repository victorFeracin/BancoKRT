namespace BancoKRT.Application.DTOs
{
    public sealed record ProcessPixTransactionRequestDto(
        string Cpf,
        string AgencyNumber,
        string AccountNumber,
        decimal Amount);
}
