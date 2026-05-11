using BancoKRT.Application.Common;
using BancoKRT.Application.DTOs;
using BancoKRT.Application.Services;
using BancoKRT.Domain.Entities;
using BancoKRT.Domain.Interfaces;
using BancoKRT.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace BancoKRT.Tests.Application.Services;

public class PixLimitAccountServiceTests
{
    private readonly Mock<IPixLimitAccountRepository> _repositoryMock = new();
    private readonly PixLimitAccountService _service;

    public PixLimitAccountServiceTests()
    {
        _service = new PixLimitAccountService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenCpfIsInvalid()
    {
        var request = new CreatePixLimitAccountRequestDto("123", "0001", "12345-6", 500m);

        var result = await _service.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        _repositoryMock.Verify(
            repository => repository.ExistsAsync(
                It.IsAny<Cpf>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenAgencyNumberIsInvalid()
    {
        var request = new CreatePixLimitAccountRequestDto("52998224725", " ", "12345-6", 500m);

        var result = await _service.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnConflict_WhenAccountAlreadyExists()
    {
        var request = new CreatePixLimitAccountRequestDto("52998224725", "0001", "12345-6", 500m);

        _repositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Conflict);
        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldFail_WhenDomainValidationFails()
    {
        var request = new CreatePixLimitAccountRequestDto("52998224725", "0001", "12345-6", -1m);

        _repositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistAccountAndReturnDto_WhenRequestIsValid()
    {
        var request = new CreatePixLimitAccountRequestDto("529.982.247-25", "0001", "12345-6", 500m);

        _repositoryMock
            .Setup(repository => repository.ExistsAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Cpf.Should().Be("52998224725");
        result.Value.AgencyNumber.Should().Be("0001");
        result.Value.AccountNumber.Should().Be("12345-6");
        result.Value.TransactionLimit.Should().Be(500m);
        result.Value.IsDeleted.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<PixLimitAccount>(account =>
                    account.Cpf.Value == "52998224725" &&
                    account.AgencyNumber == "0001" &&
                    account.AccountNumber == "12345-6" &&
                    account.TransactionLimit == 500m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldFail_WhenCpfIsInvalid()
    {
        var result = await _service.GetByAccountAsync("123", "0001", "12345-6");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldFail_WhenAgencyNumberIsInvalid()
    {
        var result = await _service.GetByAccountAsync("52998224725", "", "12345-6");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PixLimitAccount?)null);

        var result = await _service.GetByAccountAsync("52998224725", "0001", "12345-6");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldReturnNotFound_WhenAccountIsDeleted()
    {
        var account = CreateAccount();
        account.Delete();

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.GetByAccountAsync("52998224725", "0001", "12345-6");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldReturnDto_WhenAccountExists()
    {
        var account = CreateAccount(transactionLimit: 500m);

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.GetByAccountAsync("52998224725", "0001", "12345-6");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Cpf.Should().Be("52998224725");
        result.Value.TransactionLimit.Should().Be(500m);
    }

    [Fact]
    public async Task UpdateLimitAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PixLimitAccount?)null);

        var result = await _service.UpdateLimitAsync(
            "52998224725",
            "0001",
            "12345-6",
            new UpdatePixLimitAccountRequestDto(900m));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateLimitAsync_ShouldFail_WhenNewLimitIsInvalid()
    {
        var account = CreateAccount(transactionLimit: 500m);

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.UpdateLimitAsync(
            "52998224725",
            "0001",
            "12345-6",
            new UpdatePixLimitAccountRequestDto(-1m));

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        account.TransactionLimit.Should().Be(500m);
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateLimitAsync_ShouldPersistUpdatedLimit_WhenRequestIsValid()
    {
        var account = CreateAccount(transactionLimit: 500m);

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.UpdateLimitAsync(
            "52998224725",
            "0001",
            "12345-6",
            new UpdatePixLimitAccountRequestDto(900m));

        result.IsSuccess.Should().BeTrue();
        result.Value!.TransactionLimit.Should().Be(900m);
        account.TransactionLimit.Should().Be(900m);
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNotFound_WhenAccountDoesNotExist()
    {
        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PixLimitAccount?)null);

        var result = await _service.DeleteAsync("52998224725", "0001", "12345-6");

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.NotFound);
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteAccountAndPersist_WhenAccountExists()
    {
        var account = CreateAccount();

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.DeleteAsync("52998224725", "0001", "12345-6");

        result.IsSuccess.Should().BeTrue();
        account.IsDeleted.Should().BeTrue();
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldFail_WhenIdentityIsInvalid()
    {
        var request = new ProcessPixTransactionRequestDto("52998224725", "", "12345-6", 100m);

        var result = await _service.ProcessTransactionAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.Validation);
        _repositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldFail_WhenLimitIsInsufficient()
    {
        var account = CreateAccount(transactionLimit: 100m);
        var request = new ProcessPixTransactionRequestDto("52998224725", "0001", "12345-6", 150m);

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.ProcessTransactionAsync(request);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ApplicationErrorType.BusinessRule);
        account.TransactionLimit.Should().Be(100m);
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(
                It.IsAny<PixLimitAccount>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessTransactionAsync_ShouldApproveAndPersist_WhenLimitIsSufficient()
    {
        var account = CreateAccount(transactionLimit: 500m);
        var request = new ProcessPixTransactionRequestDto("52998224725", "0001", "12345-6", 120m);

        _repositoryMock
            .Setup(repository => repository.GetByAccountAsync(
                It.IsAny<Cpf>(),
                "0001",
                "12345-6",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _service.ProcessTransactionAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Approved.Should().BeTrue();
        result.Value.RemainingLimit.Should().Be(380m);
        account.TransactionLimit.Should().Be(380m);
        _repositoryMock.Verify(
            repository => repository.UpdateAsync(account, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static PixLimitAccount CreateAccount(
        string cpf = "52998224725",
        string agencyNumber = "0001",
        string accountNumber = "12345-6",
        decimal transactionLimit = 500m)
    {
        return PixLimitAccount.Create(
            Cpf.Create(cpf).Value!,
            agencyNumber,
            accountNumber,
            transactionLimit).Value!;
    }
}
