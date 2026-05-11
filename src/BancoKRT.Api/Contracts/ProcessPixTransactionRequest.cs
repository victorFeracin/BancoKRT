using System.ComponentModel.DataAnnotations;

namespace BancoKRT.Api.Contracts;

public sealed class ProcessPixTransactionRequest
{
    [Required(ErrorMessage = "O CPF é obrigatório.")]
    public string Cpf { get; init; } = string.Empty;

    [Required(ErrorMessage = "A agência é obrigatória.")]
    public string AgencyNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "A conta é obrigatória.")]
    public string AccountNumber { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "O valor da transação deve ser maior que zero.")]
    public decimal Amount { get; init; }
}
