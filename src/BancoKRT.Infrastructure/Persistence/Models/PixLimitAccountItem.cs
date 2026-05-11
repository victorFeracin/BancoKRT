namespace BancoKRT.Infrastructure.Persistence.Models
{
    public sealed class PixLimitAccountItem
    {
        public string Pk { get; set; } = string.Empty;

        public string Sk { get; set; } = string.Empty;

        public string Cpf { get; set; } = string.Empty;

        public string AgencyNumber { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;

        public decimal TransactionLimit { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
