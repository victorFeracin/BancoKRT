using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.ValueObjects;
using BancoKRT.Infrastructure.Configuration;
using BancoKRT.Infrastructure.Persistence.Mappers;
using Microsoft.Extensions.Options;

namespace BancoKRT.Infrastructure.Repositories
{
    public sealed class PixLimitAccountRepository : IPixLimitAccountRepository
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly DynamoDbOptions _options;

        public PixLimitAccountRepository(
            IAmazonDynamoDB dynamoDb,
            IOptions<DynamoDbOptions> options)
        {
            _dynamoDb = dynamoDb;
            _options = options.Value;
        }

        public async Task AddAsync(PixLimitAccount account, CancellationToken cancellationToken = default)
        {
            var item = PixLimitAccountMapper.ToItem(account);

            await _dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = _options.TableName,
                Item = PixLimitAccountMapper.ToAttributes(item),
                ConditionExpression = "attribute_not_exists(PK) AND attribute_not_exists(SK)"
            }, cancellationToken);
        }

        public async Task<PixLimitAccount?> GetByAccountAsync(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default)
        {
            var response = await _dynamoDb.GetItemAsync(new GetItemRequest
            {
                TableName = _options.TableName,
                ConsistentRead = true,
                Key = BuildKey(cpf, agencyNumber, accountNumber)
            }, cancellationToken);

            var item = PixLimitAccountMapper.FromAttributes(response.Item);

            return item is null
                ? null
                : PixLimitAccountMapper.ToDomain(item);
        }

        public async Task<bool> ExistsAsync(
            Cpf cpf,
            string agencyNumber,
            string accountNumber,
            CancellationToken cancellationToken = default)
        {
            var response = await _dynamoDb.GetItemAsync(new GetItemRequest
            {
                TableName = _options.TableName,
                ConsistentRead = true,
                Key = BuildKey(cpf, agencyNumber, accountNumber),
                ProjectionExpression = "PK"
            }, cancellationToken);

            // Itens com soft delete continuam existindo fisicamente para preservar histórico
            // e evitar recriação implícita do mesmo agregado por sobrescrita.
            return response.Item.Count > 0;
        }

        public async Task UpdateAsync(PixLimitAccount account, CancellationToken cancellationToken = default)
        {
            var item = PixLimitAccountMapper.ToItem(account);

            await _dynamoDb.PutItemAsync(new PutItemRequest
            {
                TableName = _options.TableName,
                Item = PixLimitAccountMapper.ToAttributes(item),
                ConditionExpression = "attribute_exists(PK) AND attribute_exists(SK)"
            }, cancellationToken);
        }

        private static Dictionary<string, AttributeValue> BuildKey(Cpf cpf, string agencyNumber, string accountNumber)
        {
            return new Dictionary<string, AttributeValue>
            {
                ["PK"] = new AttributeValue(PixLimitAccountMapper.BuildPartitionKey(cpf.Value)),
                ["SK"] = new AttributeValue(PixLimitAccountMapper.BuildSortKey(agencyNumber, accountNumber))
            };
        }
    }
}
