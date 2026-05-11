namespace BancoKRT.Application.DTOs
{
    public sealed record PixLimitAccountResponseDto(
        string Cpf,
        string AgencyNumber,
        string AccountNumber,
        decimal TransactionLimit,
        DateTime CreatedAt,
        bool IsDeleted,
        DateTime? DeletedAt);
}
