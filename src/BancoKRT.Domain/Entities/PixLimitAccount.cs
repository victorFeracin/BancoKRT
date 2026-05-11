using BancoKRT.Domain.Abstractions;
using BancoKRT.Domain.Common;
using BancoKRT.Domain.ValueObjects;

namespace BancoKRT.Domain.Entities
{
    public class PixLimitAccount : EntityBase
    {
        public Cpf Cpf { get; private set; }
        public string AgencyNumber { get; private set; }
        public string AccountNumber { get; private set; }
        public decimal TransactionLimit { get; private set; }

        private PixLimitAccount(Cpf cpf, string agencyNumber, string accountNumber, decimal transactionLimit)
        {
            Cpf = cpf;
            AgencyNumber = agencyNumber;
            AccountNumber = accountNumber;
            TransactionLimit = transactionLimit;
        }

        public static DomainResult<PixLimitAccount> Create(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            decimal transactionLimit)
        {
            var identityValidation = ValidateIdentity(cpf, agencyNumber, accountNumber);

            if (identityValidation.IsFailure)
            {
                return Failure<PixLimitAccount>(identityValidation);
            }

            if (transactionLimit < 0)
            {
                return DomainResult<PixLimitAccount>.Failure(
                    DomainErrorType.Validation,
                    "O limite da conta não pode ser negativo.");
            }

            return DomainResult<PixLimitAccount>.Success(
                new PixLimitAccount(cpf, agencyNumber, accountNumber, transactionLimit));
        }

        public static DomainResult<PixLimitAccount> Rehydrate(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            decimal transactionLimit,
            DateTime createdAt,
            bool isDeleted,
            DateTime? deletedAt)
        {
            var createResult = Create(cpf, agencyNumber, accountNumber, transactionLimit);

            if (createResult.IsFailure)
            {
                return createResult;
            }

            if (createdAt == default)
            {
                return DomainResult<PixLimitAccount>.Failure(
                    DomainErrorType.Validation,
                    "A data de criação é obrigatória para reidratação.");
            }

            if (isDeleted && deletedAt is null)
            {
                return DomainResult<PixLimitAccount>.Failure(
                    DomainErrorType.Validation,
                    "A data de exclusão é obrigatória para registros removidos.");
            }

            if (!isDeleted && deletedAt is not null)
            {
                return DomainResult<PixLimitAccount>.Failure(
                    DomainErrorType.Validation,
                    "Registros ativos não podem possuir data de exclusão.");
            }

            var account = createResult.Value!;
            account.RestoreState(createdAt, isDeleted, deletedAt);

            return DomainResult<PixLimitAccount>.Success(account);
        }

        public DomainResult<bool> HasSufficientLimit(decimal amount)
        {
            var activeValidation = EnsureEntityIsActive();

            if (activeValidation.IsFailure)
            {
                return Failure<bool>(activeValidation);
            }

            return DomainResult<bool>.Success(TransactionLimit >= amount);
        }

        public DomainResult DeductLimit(decimal amount)
        {
            var activeValidation = EnsureEntityIsActive();

            if (activeValidation.IsFailure)
            {
                return activeValidation;
            }

            if (amount <= 0)
            {
                return DomainResult.Failure(
                    DomainErrorType.Validation,
                    "O valor da transação deve ser maior que zero.");
            }

            var sufficientLimitResult = HasSufficientLimit(amount);

            if (sufficientLimitResult.IsFailure)
            {
                return DomainResult.Failure(
                    sufficientLimitResult.Error!.Type,
                    sufficientLimitResult.Error.Message);
            }

            if (!sufficientLimitResult.Value!)
            {
                return DomainResult.Failure(
                    DomainErrorType.BusinessRule,
                    "Limite insuficiente para realizar a transação.");
            }

            TransactionLimit -= amount;
            return DomainResult.Success();
        }

        public DomainResult UpdateLimit(decimal newLimit)
        {
            var activeValidation = EnsureEntityIsActive();

            if (activeValidation.IsFailure)
            {
                return activeValidation;
            }

            if (newLimit < 0)
            {
                return DomainResult.Failure(
                    DomainErrorType.Validation,
                    "O novo limite não pode ser negativo.");
            }

            TransactionLimit = newLimit;
            return DomainResult.Success();
        }

        private static DomainResult ValidateIdentity(Cpf cpf, string agencyNumber, string accountNumber)
        {
            if (cpf is null)
            {
                return DomainResult.Failure(
                    DomainErrorType.Validation,
                    "O CPF é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(agencyNumber))
            {
                return DomainResult.Failure(
                    DomainErrorType.Validation,
                    "O número da agência é obrigatório.");
            }

            if (string.IsNullOrWhiteSpace(accountNumber))
            {
                return DomainResult.Failure(
                    DomainErrorType.Validation,
                    "O número da conta é obrigatório.");
            }

            return DomainResult.Success();
        }

        private DomainResult EnsureEntityIsActive()
        {
            if (IsDeleted)
            {
                return DomainResult.Failure(
                    DomainErrorType.BusinessRule,
                    "Não é possível operar em um registro removido.");
            }

            return DomainResult.Success();
        }

        private static DomainResult<T> Failure<T>(DomainResult result)
        {
            return DomainResult<T>.Failure(result.Error!.Type, result.Error.Message);
        }
    }
}
