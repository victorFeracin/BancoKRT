using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.ValueObjects;
using BancoKRT.Infrastructure.Configuration;
using BancoKRT.Infrastructure.Persistence.Mappers;
using BancoKRT.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace BancoKRT.Tests.Infrastructure.Repositories;

public class PixLimitAccountRepositoryTests
{
    private readonly Mock<IAmazonDynamoDB> _dynamoDbMock = new(MockBehavior.Strict);
    private readonly PixLimitAccountRepository _repository;

    public PixLimitAccountRepositoryTests()
    {
        _repository = new PixLimitAccountRepository(
            _dynamoDbMock.Object,
            Options.Create(new DynamoDbOptions { TableName = "PixLimitAccounts" }));
    }

    [Fact]
    public async Task AddAsync_ShouldSendConditionalPutRequest()
    {
        var account = CreateAccount();
        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutItemResponse());

        await _repository.AddAsync(account);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.TableName.Should().Be("PixLimitAccounts");
        capturedRequest.ConditionExpression.Should().Be("attribute_not_exists(PK) AND attribute_not_exists(SK)");
        capturedRequest.Item["PK"].S.Should().Be("CPF#52998224725");
        capturedRequest.Item["SK"].S.Should().Be("ACCOUNT#0001#12345-6");
        capturedRequest.Item["TransactionLimit"].N.Should().Be("500");
        _dynamoDbMock.VerifyAll();
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldReturnMappedAccount_WhenItemExists()
    {
        GetItemRequest? capturedRequest = null;
        var item = PixLimitAccountMapper.ToAttributes(PixLimitAccountMapper.ToItem(CreateAccount()));

        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.GetItemAsync(
                It.IsAny<GetItemRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<GetItemRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new GetItemResponse { Item = item });

        var account = await _repository.GetByAccountAsync(
            Cpf.Create("52998224725").Value!,
            "0001",
            "12345-6");

        account.Should().NotBeNull();
        account!.Cpf.Value.Should().Be("52998224725");
        account.AgencyNumber.Should().Be("0001");
        account.AccountNumber.Should().Be("12345-6");
        account.TransactionLimit.Should().Be(500m);
        capturedRequest.Should().NotBeNull();
        capturedRequest!.TableName.Should().Be("PixLimitAccounts");
        capturedRequest.ConsistentRead.Should().BeTrue();
        capturedRequest.Key["PK"].S.Should().Be("CPF#52998224725");
        capturedRequest.Key["SK"].S.Should().Be("ACCOUNT#0001#12345-6");
        _dynamoDbMock.VerifyAll();
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.GetItemAsync(
                It.IsAny<GetItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        var account = await _repository.GetByAccountAsync(
            Cpf.Create("52998224725").Value!,
            "0001",
            "12345-6");

        account.Should().BeNull();
        _dynamoDbMock.VerifyAll();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenItemExists()
    {
        GetItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.GetItemAsync(
                It.IsAny<GetItemRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<GetItemRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new GetItemResponse
            {
                Item = new Dictionary<string, AttributeValue>
                {
                    ["PK"] = new("CPF#52998224725")
                }
            });

        var exists = await _repository.ExistsAsync(
            Cpf.Create("52998224725").Value!,
            "0001",
            "12345-6");

        exists.Should().BeTrue();
        capturedRequest.Should().NotBeNull();
        capturedRequest!.ProjectionExpression.Should().Be("PK");
        capturedRequest.ConsistentRead.Should().BeTrue();
        _dynamoDbMock.VerifyAll();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.GetItemAsync(
                It.IsAny<GetItemRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetItemResponse { Item = new Dictionary<string, AttributeValue>() });

        var exists = await _repository.ExistsAsync(
            Cpf.Create("52998224725").Value!,
            "0001",
            "12345-6");

        exists.Should().BeFalse();
        _dynamoDbMock.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_ShouldSendConditionalPutRequest()
    {
        var account = CreateAccount();
        PutItemRequest? capturedRequest = null;

        _dynamoDbMock
            .Setup(dynamoDb => dynamoDb.PutItemAsync(
                It.IsAny<PutItemRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<PutItemRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutItemResponse());

        await _repository.UpdateAsync(account);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.TableName.Should().Be("PixLimitAccounts");
        capturedRequest.ConditionExpression.Should().Be("attribute_exists(PK) AND attribute_exists(SK)");
        capturedRequest.Item["PK"].S.Should().Be("CPF#52998224725");
        capturedRequest.Item["SK"].S.Should().Be("ACCOUNT#0001#12345-6");
        _dynamoDbMock.VerifyAll();
    }

    private static PixLimitAccount CreateAccount()
    {
        return PixLimitAccount.Create(
            Cpf.Create("52998224725").Value!,
            "0001",
            "12345-6",
            500m).Value!;
    }
}
