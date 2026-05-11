namespace BancoKRT.Application.DTOs
{
    public sealed record ProcessPixTransactionResponseDto(
        bool Approved,
        string Message,
        decimal RemainingLimit);
}
