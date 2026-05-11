using System.Globalization;
using Amazon.DynamoDBv2.Model;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.ValueObjects;
using BancoKRT.Infrastructure.Persistence.Models;

namespace BancoKRT.Infrastructure.Persistence.Mappers
{
    internal static class PixLimitAccountMapper
    {
        public static PixLimitAccountItem ToItem(PixLimitAccount account)
        {
            return new PixLimitAccountItem
            {
                Pk = BuildPartitionKey(account.Cpf.Value),
                Sk = BuildSortKey(account.AgencyNumber, account.AccountNumber),
                Cpf = account.Cpf.Value,
                AgencyNumber = account.AgencyNumber,
                AccountNumber = account.AccountNumber,
                TransactionLimit = account.TransactionLimit,
                CreatedAt = account.CreatedAt.ToUniversalTime(),
                IsDeleted = account.IsDeleted,
                DeletedAt = account.DeletedAt?.ToUniversalTime()
            };
        }

        public static PixLimitAccount ToDomain(PixLimitAccountItem item)
        {
            var cpfResult = Cpf.Create(item.Cpf);

            if (cpfResult.IsFailure)
            {
                throw new InvalidOperationException($"CPF persistido inválido para o item {item.Pk}/{item.Sk}.");
            }

            var accountResult = PixLimitAccount.Rehydrate(
                cpfResult.Value!,
                item.AgencyNumber,
                item.AccountNumber,
                item.TransactionLimit,
                item.CreatedAt.ToUniversalTime(),
                item.IsDeleted,
                item.DeletedAt?.ToUniversalTime());

            if (accountResult.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Item persistido inválido para o agregado PixLimitAccount {item.Pk}/{item.Sk}: {accountResult.Error!.Message}");
            }

            return accountResult.Value!;
        }

        public static Dictionary<string, AttributeValue> ToAttributes(PixLimitAccountItem item)
        {
            var attributes = new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue(item.Pk),
                ["SK"] = new AttributeValue(item.Sk),
                ["Cpf"] = new AttributeValue(item.Cpf),
                ["AgencyNumber"] = new AttributeValue(item.AgencyNumber),
                ["AccountNumber"] = new AttributeValue(item.AccountNumber),
                ["TransactionLimit"] = new AttributeValue
                {
                    N = item.TransactionLimit.ToString(CultureInfo.InvariantCulture)
                },
                ["CreatedAt"] = new AttributeValue(item.CreatedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)),
                ["IsDeleted"] = new AttributeValue
                {
                    BOOL = item.IsDeleted
                }
            };

            if (item.DeletedAt is not null)
            {
                attributes["DeletedAt"] = new AttributeValue(
                    item.DeletedAt.Value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            }

            return attributes;
        }

        public static PixLimitAccountItem? FromAttributes(Dictionary<string, AttributeValue>? attributes)
        {
            if (attributes is null || attributes.Count == 0)
            {
                return null;
            }

            return new PixLimitAccountItem
            {
                Pk = GetRequiredString(attributes, "PK"),
                Sk = GetRequiredString(attributes, "SK"),
                Cpf = GetRequiredString(attributes, "Cpf"),
                AgencyNumber = GetRequiredString(attributes, "AgencyNumber"),
                AccountNumber = GetRequiredString(attributes, "AccountNumber"),
                TransactionLimit = decimal.Parse(GetRequiredNumber(attributes, "TransactionLimit"), CultureInfo.InvariantCulture),
                CreatedAt = ParseUtcDateTime(GetRequiredString(attributes, "CreatedAt")),
                IsDeleted = GetRequiredBool(attributes, "IsDeleted"),
                DeletedAt = attributes.TryGetValue("DeletedAt", out var deletedAt)
                    ? ParseUtcDateTime(deletedAt.S)
                    : null
            };
        }

        public static string BuildPartitionKey(string cpf)
        {
            return $"CPF#{cpf}";
        }

        public static string BuildSortKey(string agencyNumber, string accountNumber)
        {
            return $"ACCOUNT#{agencyNumber}#{accountNumber}";
        }

        private static string GetRequiredString(IReadOnlyDictionary<string, AttributeValue> attributes, string attributeName)
        {
            if (!attributes.TryGetValue(attributeName, out var attribute) || string.IsNullOrWhiteSpace(attribute.S))
            {
                throw new InvalidOperationException($"Atributo obrigatório ausente ou inválido: {attributeName}.");
            }

            return attribute.S;
        }

        private static string GetRequiredNumber(IReadOnlyDictionary<string, AttributeValue> attributes, string attributeName)
        {
            if (!attributes.TryGetValue(attributeName, out var attribute) || string.IsNullOrWhiteSpace(attribute.N))
            {
                throw new InvalidOperationException($"Atributo numérico obrigatório ausente ou inválido: {attributeName}.");
            }

            return attribute.N;
        }

        private static bool GetRequiredBool(IReadOnlyDictionary<string, AttributeValue> attributes, string attributeName)
        {
            if (!attributes.TryGetValue(attributeName, out var attribute) || attribute.BOOL is null)
            {
                throw new InvalidOperationException($"Atributo booleano obrigatório ausente ou inválido: {attributeName}.");
            }

            return attribute.BOOL.Value;
        }

        private static DateTime ParseUtcDateTime(string value)
        {
            return DateTime.Parse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind).ToUniversalTime();
        }
    }
}
