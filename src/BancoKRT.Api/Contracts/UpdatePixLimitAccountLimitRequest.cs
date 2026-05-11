using System.ComponentModel.DataAnnotations;

namespace BancoKRT.Api.Contracts;

public sealed class UpdatePixLimitAccountLimitRequest
{
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "O limite da transação deve ser maior que zero.")]
    public decimal TransactionLimit { get; init; }
}
