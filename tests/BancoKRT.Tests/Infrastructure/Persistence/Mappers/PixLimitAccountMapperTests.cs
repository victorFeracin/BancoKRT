using Amazon.DynamoDBv2.Model;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.ValueObjects;
using BancoKRT.Infrastructure.Persistence.Mappers;
using BancoKRT.Infrastructure.Persistence.Models;
using FluentAssertions;

namespace BancoKRT.Tests.Infrastructure.Persistence.Mappers;

public class PixLimitAccountMapperTests
{
    [Fact]
    public void ToItem_ShouldMapDomainAccountToPersistenceItem()
    {
        var account = CreateAccount();
        account.Delete();

        var item = PixLimitAccountMapper.ToItem(account);

        item.Pk.Should().Be("CPF#52998224725");
        item.Sk.Should().Be("ACCOUNT#0001#12345-6");
        item.Cpf.Should().Be("52998224725");
        item.AgencyNumber.Should().Be("0001");
        item.AccountNumber.Should().Be("12345-6");
        item.TransactionLimit.Should().Be(500m);
        item.IsDeleted.Should().BeTrue();
        item.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        item.DeletedAt.Should().NotBeNull();
        item.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void ToAttributes_AndFromAttributes_ShouldRoundTripPersistenceItem()
    {
        var deletedAt = new DateTime(2026, 05, 10, 18, 30, 0, DateTimeKind.Local);
        var item = new PixLimitAccountItem
        {
            Pk = "CPF#52998224725",
            Sk = "ACCOUNT#0001#12345-6",
            Cpf = "52998224725",
            AgencyNumber = "0001",
            AccountNumber = "12345-6",
            TransactionLimit = 725.45m,
            CreatedAt = new DateTime(2026, 05, 10, 17, 15, 0, DateTimeKind.Local),
            IsDeleted = true,
            DeletedAt = deletedAt
        };

        var attributes = PixLimitAccountMapper.ToAttributes(item);
        var roundTrip = PixLimitAccountMapper.FromAttributes(attributes);

        roundTrip.Should().NotBeNull();
        roundTrip!.Pk.Should().Be(item.Pk);
        roundTrip.Sk.Should().Be(item.Sk);
        roundTrip.Cpf.Should().Be(item.Cpf);
        roundTrip.AgencyNumber.Should().Be(item.AgencyNumber);
        roundTrip.AccountNumber.Should().Be(item.AccountNumber);
        roundTrip.TransactionLimit.Should().Be(item.TransactionLimit);
        roundTrip.IsDeleted.Should().BeTrue();
        roundTrip.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        roundTrip.DeletedAt.Should().NotBeNull();
        roundTrip.DeletedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        attributes["DeletedAt"].S.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ToDomain_ShouldRehydratePersistedAccount()
    {
        var item = new PixLimitAccountItem
        {
            Pk = "CPF#52998224725",
            Sk = "ACCOUNT#0001#12345-6",
            Cpf = "52998224725",
            AgencyNumber = "0001",
            AccountNumber = "12345-6",
            TransactionLimit = 900m,
            CreatedAt = new DateTime(2026, 05, 10, 20, 0, 0, DateTimeKind.Utc),
            IsDeleted = true,
            DeletedAt = new DateTime(2026, 05, 10, 21, 0, 0, DateTimeKind.Utc)
        };

        var account = PixLimitAccountMapper.ToDomain(item);

        account.Cpf.Value.Should().Be("52998224725");
        account.AgencyNumber.Should().Be("0001");
        account.AccountNumber.Should().Be("12345-6");
        account.TransactionLimit.Should().Be(900m);
        account.CreatedAt.Should().Be(item.CreatedAt);
        account.IsDeleted.Should().BeTrue();
        account.DeletedAt.Should().Be(item.DeletedAt);
    }

    [Fact]
    public void ToDomain_ShouldThrow_WhenCpfIsInvalid()
    {
        var item = new PixLimitAccountItem
        {
            Pk = "CPF#123",
            Sk = "ACCOUNT#0001#12345-6",
            Cpf = "123",
            AgencyNumber = "0001",
            AccountNumber = "12345-6",
            TransactionLimit = 900m,
            CreatedAt = new DateTime(2026, 05, 10, 20, 0, 0, DateTimeKind.Utc)
        };

        var act = () => PixLimitAccountMapper.ToDomain(item);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*CPF persistido inv*");
    }

    [Fact]
    public void FromAttributes_ShouldReturnNull_WhenAttributesAreMissing()
    {
        PixLimitAccountMapper.FromAttributes(null).Should().BeNull();
        PixLimitAccountMapper.FromAttributes(new Dictionary<string, AttributeValue>()).Should().BeNull();
    }

    [Fact]
    public void FromAttributes_ShouldThrow_WhenRequiredStringAttributeIsMissing()
    {
        var attributes = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new("CPF#52998224725"),
            ["SK"] = new("ACCOUNT#0001#12345-6"),
            ["Cpf"] = new("52998224725"),
            ["AgencyNumber"] = new(string.Empty),
            ["AccountNumber"] = new("12345-6"),
            ["TransactionLimit"] = new AttributeValue { N = "500" },
            ["CreatedAt"] = new("2026-05-10T20:00:00.0000000Z"),
            ["IsDeleted"] = new AttributeValue { BOOL = false }
        };

        var act = () => PixLimitAccountMapper.FromAttributes(attributes);

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*AgencyNumber*");
    }

    [Fact]
    public void BuildKeys_ShouldReturnExpectedPatterns()
    {
        PixLimitAccountMapper.BuildPartitionKey("52998224725").Should().Be("CPF#52998224725");
        PixLimitAccountMapper.BuildSortKey("0001", "12345-6").Should().Be("ACCOUNT#0001#12345-6");
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
