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

public class PixTransactionsControllerTests
{
    private readonly Mock<IPixLimitAccountService> _serviceMock = new();
    private readonly PixTransactionsController _controller;

    public PixTransactionsControllerTests()
    {
        _controller = new PixTransactionsController(_serviceMock.Object);
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnOk_WhenServiceApprovesTransaction()
    {
        var request = new ProcessPixTransactionRequest
        {
            Cpf = "52998224725",
            AgencyNumber = "0001",
            AccountNumber = "12345-6",
            Amount = 120m
        };

        var response = new ProcessPixTransactionResponseDto(true, "Transação aprovada.", 380m);

        _serviceMock
            .Setup(service => service.ProcessTransactionAsync(
                It.Is<ProcessPixTransactionRequestDto>(dto =>
                    dto.Cpf == request.Cpf &&
                    dto.AgencyNumber == request.AgencyNumber &&
                    dto.AccountNumber == request.AccountNumber &&
                    dto.Amount == request.Amount),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationResult<ProcessPixTransactionResponseDto>.Success(response));

        var result = await _controller.ProcessAsync(request, CancellationToken.None);

        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task ProcessAsync_ShouldReturnUnprocessableEntity_WhenBusinessRuleFails()
    {
        var request = new ProcessPixTransactionRequest
        {
            Cpf = "52998224725",
            AgencyNumber = "0001",
            AccountNumber = "12345-6",
            Amount = 600m
        };

        _serviceMock
            .Setup(service => service.ProcessTransactionAsync(
                It.IsAny<ProcessPixTransactionRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(ApplicationResult<ProcessPixTransactionResponseDto>.Failure(
                ApplicationErrorType.BusinessRule,
                "Limite PIX insuficiente."));

        var result = await _controller.ProcessAsync(request, CancellationToken.None);

        var objectResult = result.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Detail.Should().Be("Limite PIX insuficiente.");
    }
}
