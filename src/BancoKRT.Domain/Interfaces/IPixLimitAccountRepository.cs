using BancoKRT.Domain.Entities;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Domain.Interfaces
{
    public interface IPixLimitAccountRepository
    {
        Task AddAsync(PixLimitAccount account, CancellationToken cancellationToken = default);

        Task<PixLimitAccount?> GetByAccountAsync(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(PixLimitAccount account, CancellationToken cancellationToken = default);
    }
}
