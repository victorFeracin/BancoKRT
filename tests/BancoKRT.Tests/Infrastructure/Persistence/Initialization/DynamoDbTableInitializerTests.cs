using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BancoKRT.Infrastructure.Configuration;
using BancoKRT.Infrastructure.Persistence.Initialization;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace BancoKRT.Tests.Infrastructure.Persistence.Initialization;

public class DynamoDbTableInitializerTests
{
    private readonly Mock<IAmazonDynamoDB> _dynamoDbMock = new(MockBehavior.Strict);

    [Fact]
    public async Task StartAsync_ShouldDoNothing_WhenTableAlreadyExists()
    {
        var initializer = CreateInitializer();

        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.DescribeTableAsync(
                "PixLimitAccounts",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DescribeTableResponse
            {
                Table = new TableDescription
                {
                    TableStatus = TableStatus.ACTIVE
                }
            });

        await initializer.StartAsync(CancellationToken.None);

        _dynamoDbMock.Verify(
            dynamoDb => dynamoDb.CreateTableAsync(
                It.IsAny<CreateTableRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _dynamoDbMock.VerifyAll();
    }

    [Fact]
    public async Task StartAsync_ShouldCreateTable_WhenTableDoesNotExist()
    {
        var initializer = CreateInitializer();
        CreateTableRequest? capturedRequest = null;

        _dynamoDbMock
            .SetupSequence(dynamoDb => dynamoDb.DescribeTableAsync(
                "PixLimitAccounts",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ResourceNotFoundException("missing"))
            .ReturnsAsync(new DescribeTableResponse
            {
                Table = new TableDescription
                {
                    TableStatus = TableStatus.ACTIVE
                }
            });

        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.CreateTableAsync(
                It.IsAny<CreateTableRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreateTableRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new CreateTableResponse());

        await initializer.StartAsync(CancellationToken.None);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.TableName.Should().Be("PixLimitAccounts");
        capturedRequest.BillingMode.Should().Be(BillingMode.PAY_PER_REQUEST);
        capturedRequest.AttributeDefinitions.Should().ContainSingle(definition =>
            definition.AttributeName == "PK" &&
            definition.AttributeType == ScalarAttributeType.S);
        capturedRequest.AttributeDefinitions.Should().ContainSingle(definition =>
            definition.AttributeName == "SK" &&
            definition.AttributeType == ScalarAttributeType.S);
        capturedRequest.KeySchema.Should().ContainSingle(element =>
            element.AttributeName == "PK" &&
            element.KeyType == KeyType.HASH);
        capturedRequest.KeySchema.Should().ContainSingle(element =>
            element.AttributeName == "SK" &&
            element.KeyType == KeyType.RANGE);
        _dynamoDbMock.Verify(
            dynamoDb => dynamoDb.DescribeTableAsync(
                "PixLimitAccounts",
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task StopAsync_ShouldCompleteSuccessfully()
    {
        var initializer = CreateInitializer();

        var act = async () => await initializer.StopAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    private DynamoDbTableInitializer CreateInitializer()
    {
        return new DynamoDbTableInitializer(
            _dynamoDbMock.Object,
            Options.Create(new DynamoDbOptions { TableName = "PixLimitAccounts" }),
            NullLogger<DynamoDbTableInitializer>.Instance);
    }
}
