using BancoKRT.Application.Common;
using BancoKRT.Application.DTOs;

namespace BancoKRT.Application.Services
{
    public interface IPixLimitAccountService
    {
        Task<ApplicationResult<PixLimitAccountResponseDto>> CreateAsync(
            CreatePixLimitAccountRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ApplicationResult<PixLimitAccountResponseDto>> GetByAccountAsync(
            string cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default);

        Task<ApplicationResult<PixLimitAccountResponseDto>> UpdateLimitAsync(
            string cpf,
            string agencyNumber,
            string accountNumber,
            UpdatePixLimitAccountRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ApplicationResult> DeleteAsync(
            string cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default);

        Task<ApplicationResult<ProcessPixTransactionResponseDto>> ProcessTransactionAsync(
            ProcessPixTransactionRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
