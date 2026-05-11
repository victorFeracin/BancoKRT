using BancoKRT.Application.Common;
using BancoKRT.Application.DTOs;
using BancoKRT.Domain.Common;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Application.Services
{
    public class PixLimitAccountService : IPixLimitAccountService
    {
        private readonly IPixLimitAccountRepository _repository;

        public PixLimitAccountService(IPixLimitAccountRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApplicationResult<PixLimitAccountResponseDto>> CreateAsync(
            CreatePixLimitAccountRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var cpfResult = CreateCpf(request.Cpf);

            if (cpfResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(cpfResult);
            }

            var cpf = cpfResult.Value!;
            var identityValidation = ValidateAccountIdentity(
                request.AgencyNumber,
                request.AccountNumber);

            if (identityValidation.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(identityValidation);
            }

            var alreadyExists = await _repository.ExistsAsync(
                cpf,
                request.AgencyNumber,
                request.AccountNumber,
                cancellationToken);

            if (alreadyExists)
            {
                return ApplicationResult<PixLimitAccountResponseDto>.Failure(
                    ApplicationErrorType.Conflict,
                    "Já existe um limite PIX cadastrado para esta conta.");
            }

            var accountResult = PixLimitAccount.Create(
                cpf,
                request.AgencyNumber,
                request.AccountNumber,
                request.TransactionLimit);

            if (accountResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(accountResult);
            }

            var account = accountResult.Value!;

            await _repository.AddAsync(account, cancellationToken);

            return ApplicationResult<PixLimitAccountResponseDto>.Success(ToResponse(account));
        }

        public async Task<ApplicationResult<PixLimitAccountResponseDto>> GetByAccountAsync(
            string cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default)
        {
            var cpfResult = CreateCpf(cpf);

            if (cpfResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(cpfResult);
            }

            var accountResult = await GetRequiredAccountAsync(
                cpfResult.Value!,
                agencyNumber,
                accountNumber,
                cancellationToken);

            if (accountResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(accountResult);
            }

            return ApplicationResult<PixLimitAccountResponseDto>.Success(ToResponse(accountResult.Value!));
        }

        public async Task<ApplicationResult<PixLimitAccountResponseDto>> UpdateLimitAsync(
            string cpf,
            string agencyNumber,
            string accountNumber,
            UpdatePixLimitAccountRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var cpfResult = CreateCpf(cpf);

            if (cpfResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(cpfResult);
            }

            var accountResult = await GetRequiredAccountAsync(
                cpfResult.Value!,
                agencyNumber,
                accountNumber,
                cancellationToken);

            if (accountResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(accountResult);
            }

            var account = accountResult.Value!;
            var updateResult = account.UpdateLimit(request.TransactionLimit);

            if (updateResult.IsFailure)
            {
                return Failure<PixLimitAccountResponseDto>(updateResult);
            }

            await _repository.UpdateAsync(account, cancellationToken);

            return ApplicationResult<PixLimitAccountResponseDto>.Success(ToResponse(account));
        }

        public async Task<ApplicationResult> DeleteAsync(
            string cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default)
        {
            var cpfResult = CreateCpf(cpf);

            if (cpfResult.IsFailure)
            {
                return ApplicationResult.Failure(cpfResult.Error!.Type, cpfResult.Error.Message);
            }

            var accountResult = await GetRequiredAccountAsync(
                cpfResult.Value!,
                agencyNumber,
                accountNumber,
                cancellationToken);

            if (accountResult.IsFailure)
            {
                return ApplicationResult.Failure(
                    accountResult.Error!.Type,
                    accountResult.Error.Message);
            }

            var account = accountResult.Value!;
            var deleteResult = account.Delete();

            if (deleteResult.IsFailure)
            {
                return Failure(deleteResult);
            }

            await _repository.UpdateAsync(account, cancellationToken);

            return ApplicationResult.Success();
        }

        public async Task<ApplicationResult<ProcessPixTransactionResponseDto>> ProcessTransactionAsync(
            ProcessPixTransactionRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var cpfResult = CreateCpf(request.Cpf);

            if (cpfResult.IsFailure)
            {
                return Failure<ProcessPixTransactionResponseDto>(cpfResult);
            }

            var cpf = cpfResult.Value!;
            var identityValidation = ValidateAccountIdentity(
                request.AgencyNumber,
                request.AccountNumber);

            if (identityValidation.IsFailure)
            {
                return Failure<ProcessPixTransactionResponseDto>(identityValidation);
            }

            var accountResult = await GetRequiredAccountAsync(
                cpf,
                request.AgencyNumber,
                request.AccountNumber,
                cancellationToken);

            if (accountResult.IsFailure)
            {
                return Failure<ProcessPixTransactionResponseDto>(accountResult);
            }

            var account = accountResult.Value!;
            var deductResult = account.DeductLimit(request.Amount);

            if (deductResult.IsFailure)
            {
                return Failure<ProcessPixTransactionResponseDto>(deductResult);
            }

            await _repository.UpdateAsync(account, cancellationToken);

            return ApplicationResult<ProcessPixTransactionResponseDto>.Success(
                new ProcessPixTransactionResponseDto(
                    true,
                    "Transação aprovada com sucesso.",
                    account.TransactionLimit));
        }

        private async Task<ApplicationResult<PixLimitAccount>> GetRequiredAccountAsync(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken)
        {
            var identityValidation = ValidateAccountIdentity(agencyNumber, accountNumber);

            if (identityValidation.IsFailure)
            {
                return Failure<PixLimitAccount>(identityValidation);
            }

            var account = await _repository.GetByAccountAsync(
                cpf,
                agencyNumber,
                accountNumber,
                cancellationToken);

            if (account is null || account.IsDeleted)
            {
                return ApplicationResult<PixLimitAccount>.Failure(
                    ApplicationErrorType.NotFound,
                    "Conta não encontrada.");
            }

            return ApplicationResult<PixLimitAccount>.Success(account);
        }

        private static ApplicationResult<Cpf> CreateCpf(string value)
        {
            var cpfResult = Cpf.Create(value);

            if (cpfResult.IsFailure)
            {
                return Failure<Cpf>(cpfResult);
            }

            return ApplicationResult<Cpf>.Success(cpfResult.Value!);
        }

        private static ApplicationResult ValidateAccountIdentity(string agencyNumber, string accountNumber)
        {
            if (string.IsNullOrWhiteSpace(agencyNumber))
            {
                return ApplicationResult.Failure(
                    ApplicationErrorType.Validation,
                    "O número da agência é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return ApplicationResult.Failure(
                    ApplicationErrorType.Validation,
                    "O número da conta é obrigatório.");
            }

            return ApplicationResult.Success();
        }

        private static ApplicationResult<T> Failure<T>(ApplicationResult result)
        {
            return ApplicationResult<T>.Failure(result.Error!.Type, result.Error.Message);
        }

        private static ApplicationResult Failure(DomainResult result)
        {
            return ApplicationResult.Failure(ToApplicationErrorType(result.Error!.Type), result.Error.Message);
        }

        private static ApplicationResult<T> Failure<T>(DomainResult result)
        {
            return ApplicationResult<T>.Failure(ToApplicationErrorType(result.Error!.Type), result.Error.Message);
        }

        private static ApplicationErrorType ToApplicationErrorType(DomainErrorType type)
        {
            return type switch
            {
                DomainErrorType.Validation => ApplicationErrorType.Validation,
                DomainErrorType.BusinessRule => ApplicationErrorType.BusinessRule,
                _ => ApplicationErrorType.BusinessRule
            };
        }

        private static PixLimitAccountResponseDto ToResponse(PixLimitAccount account)
        {
            return new PixLimitAccountResponseDto(
                account.Cpf.Value,
                account.AgencyNumber,
                account.AccountNumber,
                account.TransactionLimit,
                account.CreatedAt,
                account.IsDeleted,
                account.DeletedAt);
        }
    }
}
