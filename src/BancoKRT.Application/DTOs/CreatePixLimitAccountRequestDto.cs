namespace BancoKRT.Application.DTOs
{
    public sealed record CreatePixLimitAccountRequestDto(
        string Cpf,
        string AgencyNumber,
        string AccountNumber,
        decimal TransactionLimit);
}
