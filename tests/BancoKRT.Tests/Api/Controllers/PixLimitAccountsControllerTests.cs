using BancoKRT.Api.Controllers;
using BancoKRT.Api.Contracts;
using BancoKRT.Application.Common;
using BancoKRT.Application.DTOs;
using BancoKRT.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BancoKRT.Tests.Api.Controllers;

public class PixLimitAccountsControllerTests
{
    private readonly Mock<IPixLimitAccountService> _serviceMock = new();
    private readonly PixLimitAccountsController _controller;

    public PixLimitAccountsControllerTests()
    {
        _controller = new PixLimitAccountsController(_serviceMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnCreatedAtAction_WhenServiceSucceeds()
    {
        var request = new CreatePixLimitAccountRequest
        {
            Cpf = "52998224725",
            AgencyNumber = "0001",
            AccountNumber = "12345-6",
            TransactionLimit = 500m
        };

        var response = new PixLimitAccountResponseDto(
            request.Cpf,
            request.AgencyNumber,
            request.AccountNumber,
            request.TransactionLimit,
            DateTime.UtcNow,
            false,
            null);

        _serviceMock
            .Setup(service => service.CreateAsync(
                It.Is<CreatePixLimitAccountRequestDto>(dto =>
                    dto.Cpf == request.Cpf &&
                    dto.AgencyNumber == request.AgencyNumber &&
                    dto.AccountNumber == request.AccountNumber &&
                    dto.TransactionLimit == request.TransactionLimit),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationResult<PixLimitAccountResponseDto>.Success(response));

        var result = await _controller.CreateAsync(request, CancellationToken.None);

        var createdAtActionResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAtActionResult.ActionName.Should().Be(nameof(PixLimitAccountsController.GetByAccountAsync));
        createdAtActionResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdAtActionResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetByAccountAsync_ShouldReturnOk_WhenServiceSucceeds()
    {
        var response = new PixLimitAccountResponseDto("52998224725", "0001", "12345-6", 500m, DateTime.UtcNow, false, null);

        _serviceMock
            .Setup(service => service.GetByAccountAsync("52998224725", "0001", "12345-6", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationResult<PixLimitAccountResponseDto>.Success(response));

        var result = await _controller.GetByAccountAsync("52998224725", "0001", "12345-6", CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task UpdateLimitAsync_ShouldReturnProblemDetails_WhenServiceFails()
    {
        var request = new UpdatePixLimitAccountLimitRequest
        {
            TransactionLimit = 900m
        };

        _serviceMock
            .Setup(service => service.UpdateLimitAsync(
                "52998224725",
                "0001",
                "12345-6",
                It.Is<UpdatePixLimitAccountRequestDto>(dto => dto.TransactionLimit == 900m),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationResult<PixLimitAccountResponseDto>.Failure(
                ApplicationErrorType.NotFound,
                "Conta não encontrada."));

        var result = await _controller.UpdateLimitAsync("52998224725", "0001", "12345-6", request, CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Detail.Should().Be("Conta não encontrada.");
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnNoContent_WhenServiceSucceeds()
    {
        _serviceMock
            .Setup(service => service.DeleteAsync("52998224725", "0001", "12345-6", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationResult.Success());

        var result = await _controller.DeleteAsync("52998224725", "0001", "12345-6", CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
    }
}
