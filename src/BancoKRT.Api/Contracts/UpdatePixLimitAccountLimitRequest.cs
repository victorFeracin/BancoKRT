using System.ComponentModel.DataAnnotations;

namespace BancoKRT.Api.Contracts;

public sealed class UpdatePixLimitAccountLimitRequest
{
    [Range(typeof(decimal), "0.01", "999999999999999", ParseLimitsInInvariantCulture = true, ErrorMessage = "O limite da transação deve ser maior que zero.")]
    public decimal TransactionLimit { get; init; }
}
