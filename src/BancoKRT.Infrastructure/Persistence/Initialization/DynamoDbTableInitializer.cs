using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BancoKRT.Infrastructure.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BancoKRT.Infrastructure.Persistence.Initialization
{
    internal sealed class DynamoDbTableInitializer : IHostedService
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly DynamoDbOptions _options;
        private readonly ILogger<DynamoDbTableInitializer> _logger;

        public DynamoDbTableInitializer(
            IAmazonDynamoDB dynamoDb,
            IOptions<DynamoDbOptions> options,
            ILogger<DynamoDbTableInitializer> logger)
        {
            _dynamoDb = dynamoDb;
            _options = options.Value;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dynamoDb.DescribeTableAsync(_options.TableName, cancellationToken);
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogInformation("Tabela DynamoDB {TableName} não encontrada. Criando tabela.", _options.TableName);

                await _dynamoDb.CreateTableAsync(new CreateTableRequest
                {
                    TableName = _options.TableName,
                    BillingMode = BillingMode.PAY_PER_REQUEST,
                    AttributeDefinitions =
                    [
                        new AttributeDefinition("PK", ScalarAttributeType.S),
                        new AttributeDefinition("SK", ScalarAttributeType.S)
                    ],
                    KeySchema =
                    [
                        new KeySchemaElement("PK", KeyType.HASH),
                        new KeySchemaElement("SK", KeyType.RANGE)
                    ]
                }, cancellationToken);

                await WaitForTableToBecomeActiveAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task WaitForTableToBecomeActiveAsync(CancellationToken cancellationToken)
        {
            const int delayInMilliseconds = 500;

            while (!cancellationToken.IsCancellationRequested)
            {
                var response = await _dynamoDb.DescribeTableAsync(_options.TableName, cancellationToken);

                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    return;
                }

                await Task.Delay(delayInMilliseconds, cancellationToken);
            }
        }
    }
}
