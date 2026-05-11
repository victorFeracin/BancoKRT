using System.ComponentModel.DataAnnotations;

namespace BancoKRT.Api.Contracts;

public sealed class CreatePixLimitAccountRequest
{
    [Required(ErrorMessage = "O CPF é obrigatório.")]
    public string Cpf { get; init; } = string.Empty;

    [Required(ErrorMessage = "A agência é obrigatória.")]
    public string AgencyNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "A conta é obrigatória.")]
    public string AccountNumber { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "O limite da transação deve ser maior que zero.")]
    public decimal TransactionLimit { get; init; }
}
