using BancoKRT.Api.Extensions;
using BancoKRT.Application.Common;
using BancoKRT.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.Tests.Api.Extensions;

public class ApplicationResultExtensionsTests
{
    private readonly TestController _controller = new();

    [Theory]
    [InlineData(ApplicationErrorType.Validation, StatusCodes.Status400BadRequest, "Erro de validação")]
    [InlineData(ApplicationErrorType.Conflict, StatusCodes.Status409Conflict, "Conflito de recurso")]
    [InlineData(ApplicationErrorType.NotFound, StatusCodes.Status404NotFound, "Recurso não encontrado")]
    [InlineData(ApplicationErrorType.BusinessRule, StatusCodes.Status422UnprocessableEntity, "Ação não permitida")]
    public void ToActionResult_ShouldMapFailureToExpectedHttpStatus(
        ApplicationErrorType errorType,
        int expectedStatusCode,
        string expectedTitle)
    {
        var result = ApplicationResult<PixLimitAccountResponseDto>.Failure(errorType, "mensagem");

        var actionResult = _controller.ToActionResult(result);

        var objectResult = actionResult.Result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatusCode);
        var problemDetails = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problemDetails.Title.Should().Be(expectedTitle);
        problemDetails.Detail.Should().Be("mensagem");
        problemDetails.Status.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void ToActionResult_ShouldReturnNoContent_WhenNonGenericResultSucceeds()
    {
        var actionResult = _controller.ToActionResult(ApplicationResult.Success());

        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ToActionResult_ShouldReturnOk_WhenGenericResultSucceeds()
    {
        var dto = new PixLimitAccountResponseDto("52998224725", "0001", "12345-6", 500m, DateTime.UtcNow, false, null);

        var actionResult = _controller.ToActionResult(ApplicationResult<PixLimitAccountResponseDto>.Success(dto));

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public void ToCreatedAtRouteResult_ShouldReturnCreatedAtRoute_WhenResultSucceeds()
    {
        var dto = new PixLimitAccountResponseDto("52998224725", "0001", "12345-6", 500m, DateTime.UtcNow, false, null);

        var actionResult = _controller.ToCreatedAtRouteResult(
            ApplicationResult<PixLimitAccountResponseDto>.Success(dto),
            "GetByAccountAsync",
            new { cpf = dto.Cpf, agencyNumber = dto.AgencyNumber, accountNumber = dto.AccountNumber });

        var createdAtRouteResult = actionResult.Result.Should().BeOfType<CreatedAtRouteResult>().Subject;
        createdAtRouteResult.RouteName.Should().Be("GetByAccountAsync");
        createdAtRouteResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdAtRouteResult.Value.Should().Be(dto);
    }

    private sealed class TestController : ControllerBase;
}
