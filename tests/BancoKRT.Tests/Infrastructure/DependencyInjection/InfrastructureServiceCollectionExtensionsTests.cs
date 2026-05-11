using Amazon.DynamoDBv2;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Infrastructure.Configuration;
using BancoKRT.Infrastructure.DependencyInjection;
using BancoKRT.Infrastructure.Persistence.Initialization;
using BancoKRT.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BancoKRT.Tests.Infrastructure.DependencyInjection;

public class InfrastructureServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfrastructure_ShouldRegisterExpectedServices_ForLocalDynamoDb()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DynamoDb:TableName"] = "LocalTable",
                ["DynamoDb:Region"] = "sa-east-1",
                ["DynamoDb:ServiceUrl"] = "http://localhost:8000",
                ["DynamoDb:AccessKey"] = "local-key",
                ["DynamoDb:SecretKey"] = "local-secret"
            })
            .Build();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<DynamoDbOptions>>().Value;
        var dynamoDb = provider.GetRequiredService<IAmazonDynamoDB>();
        var repository = provider.GetRequiredService<IPixLimitAccountRepository>();
        var hostedServices = provider.GetServices<IHostedService>();

        options.TableName.Should().Be("LocalTable");
        options.Region.Should().Be("sa-east-1");
        options.ServiceUrl.Should().Be("http://localhost:8000");
        dynamoDb.Should().BeOfType<AmazonDynamoDBClient>();
        repository.Should().BeOfType<PixLimitAccountRepository>();
        hostedServices.Should().ContainSingle(service => service is DynamoDbTableInitializer);
    }

    [Fact]
    public void AddInfrastructure_ShouldResolveAmazonClient_WhenUsingAwsRegionConfiguration()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DynamoDb:TableName"] = "AwsTable",
                ["DynamoDb:Region"] = "us-east-1",
                ["DynamoDb:AccessKey"] = "aws-key",
                ["DynamoDb:SecretKey"] = "aws-secret"
            })
            .Build();

        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        var firstClient = provider.GetRequiredService<IAmazonDynamoDB>();
        var secondClient = provider.GetRequiredService<IAmazonDynamoDB>();

        firstClient.Should().BeSameAs(secondClient);
        firstClient.Should().BeOfType<AmazonDynamoDBClient>();
    }
}
