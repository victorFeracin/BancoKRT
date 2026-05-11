using BancoKRT.Domain.Common;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.ValueObjects;
using FluentAssertions;

namespace BancoKRT.Tests.Domain.Entities;

public class PixLimitAccountTests
{
    [Fact]
    public void Create_ShouldReturnAccount_WhenDataIsValid()
    {
        var cpf = CreateValidCpf();

        var result = PixLimitAccount.Create(cpf, "0001", "12345-6", 500m);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Cpf.Should().Be(cpf);
        result.Value.AgencyNumber.Should().Be("0001");
        result.Value.AccountNumber.Should().Be("12345-6");
        result.Value.TransactionLimit.Should().Be(500m);
        result.Value.IsDeleted.Should().BeFalse();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Rehydrate_ShouldRestorePersistedState_WhenDataIsValid()
    {
        var createdAt = new DateTime(2026, 05, 10, 12, 00, 00, DateTimeKind.Utc);
        var deletedAt = new DateTime(2026, 05, 11, 15, 30, 00, DateTimeKind.Utc);

        var result = PixLimitAccount.Rehydrate(
            CreateValidCpf(),
            "0001",
            "12345-6",
            500m,
            createdAt,
            true,
            deletedAt);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CreatedAt.Should().Be(createdAt);
        result.Value.IsDeleted.Should().BeTrue();
        result.Value.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public void Rehydrate_ShouldFail_WhenDeletedRecordHasNoDeletedAt()
    {
        var result = PixLimitAccount.Rehydrate(
            CreateValidCpf(),
            "0001",
            "12345-6",
            500m,
            new DateTime(2026, 05, 10, 12, 00, 00, DateTimeKind.Utc),
            true,
            null);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenCpfIsNull()
    {
        var result = PixLimitAccount.Create(null!, "0001", "12345-6", 500m);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenAgencyIsInvalid(string agencyNumber)
    {
        var result = PixLimitAccount.Create(CreateValidCpf(), agencyNumber, "12345-6", 500m);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenAccountIsInvalid(string accountNumber)
    {
        var result = PixLimitAccount.Create(CreateValidCpf(), "0001", accountNumber, 500m);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Fact]
    public void Create_ShouldFail_WhenTransactionLimitIsNegative()
    {
        var result = PixLimitAccount.Create(CreateValidCpf(), "0001", "12345-6", -1m);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
    }

    [Theory]
    [InlineData(100, true)]
    [InlineData(600, false)]
    public void HasSufficientLimit_ShouldReturnWhetherLimitCoversAmount(decimal amount, bool expected)
    {
        var account = CreateValidAccount(500m);

        var result = account.HasSufficientLimit(amount);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void DeductLimit_ShouldReduceTransactionLimit_WhenAmountIsValid()
    {
        var account = CreateValidAccount(500m);

        var result = account.DeductLimit(125m);

        result.IsSuccess.Should().BeTrue();
        account.TransactionLimit.Should().Be(375m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeductLimit_ShouldFail_WhenAmountIsNotPositive(decimal amount)
    {
        var account = CreateValidAccount(500m);

        var result = account.DeductLimit(amount);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
        account.TransactionLimit.Should().Be(500m);
    }

    [Fact]
    public void DeductLimit_ShouldFail_WhenThereIsNotEnoughLimit()
    {
        var account = CreateValidAccount(500m);

        var result = account.DeductLimit(501m);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.BusinessRule);
        account.TransactionLimit.Should().Be(500m);
    }

    [Fact]
    public void UpdateLimit_ShouldChangeTransactionLimit_WhenNewLimitIsValid()
    {
        var account = CreateValidAccount(500m);

        var result = account.UpdateLimit(900m);

        result.IsSuccess.Should().BeTrue();
        account.TransactionLimit.Should().Be(900m);
    }

    [Fact]
    public void UpdateLimit_ShouldFail_WhenNewLimitIsNegative()
    {
        var account = CreateValidAccount(500m);

        var result = account.UpdateLimit(-1m);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.Validation);
        account.TransactionLimit.Should().Be(500m);
    }

    [Fact]
    public void Delete_ShouldMarkAccountAsDeleted()
    {
        var account = CreateValidAccount();

        var result = account.Delete();

        result.IsSuccess.Should().BeTrue();
        account.IsDeleted.Should().BeTrue();
        account.DeletedAt.Should().NotBeNull();
        account.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Delete_ShouldFail_WhenAccountWasAlreadyDeleted()
    {
        var account = CreateValidAccount();
        account.Delete();

        var result = account.Delete();

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(DomainErrorType.BusinessRule);
    }

    [Fact]
    public void Operations_ShouldFail_WhenAccountIsDeleted()
    {
        var account = CreateValidAccount(500m);
        account.Delete();

        var hasSufficientLimitResult = account.HasSufficientLimit(100m);
        var deductLimitResult = account.DeductLimit(100m);
        var updateLimitResult = account.UpdateLimit(700m);

        hasSufficientLimitResult.IsFailure.Should().BeTrue();
        hasSufficientLimitResult.Error!.Type.Should().Be(DomainErrorType.BusinessRule);

        deductLimitResult.IsFailure.Should().BeTrue();
        deductLimitResult.Error!.Type.Should().Be(DomainErrorType.BusinessRule);

        updateLimitResult.IsFailure.Should().BeTrue();
        updateLimitResult.Error!.Type.Should().Be(DomainErrorType.BusinessRule);
    }

    private static PixLimitAccount CreateValidAccount(decimal transactionLimit = 500m)
    {
        return PixLimitAccount.Create(CreateValidCpf(), "0001", "12345-6", transactionLimit).Value!;
    }

    private static Cpf CreateValidCpf()
    {
        return Cpf.Create("529.982.247-25").Value!;
    }
}
