using BancoKRT.Api.Contracts;
using BancoKRT.Api.Extensions;
using BancoKRT.Application.DTOs;
using BancoKRT.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.Api.Controllers;

[ApiController]
[Route("api/limit")]
[Produces("application/json")]
public sealed class PixLimitAccountsController(IPixLimitAccountService pixLimitAccountService) : ControllerBase
{
    private const string GetByAccountRouteName = nameof(GetByAccountAsync);

    [HttpPost]
    [ProducesResponseType(typeof(PixLimitAccountResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PixLimitAccountResponseDto>> CreateAsync(
        [FromBody] CreatePixLimitAccountRequest request,
        CancellationToken cancellationToken)
    {
        var result = await pixLimitAccountService.CreateAsync(
            new CreatePixLimitAccountRequestDto(
                request.Cpf,
                request.AgencyNumber,
                request.AccountNumber,
                request.TransactionLimit),
            cancellationToken);

        return this.ToCreatedAtRouteResult(
            result,
            GetByAccountRouteName,
            new
            {
                cpf = result.Value?.Cpf ?? request.Cpf,
                agencyNumber = result.Value?.AgencyNumber ?? request.AgencyNumber,
                accountNumber = result.Value?.AccountNumber ?? request.AccountNumber
            });
    }

    [HttpGet("{cpf}/{agencyNumber}/{accountNumber}", Name = GetByAccountRouteName)]
    [ProducesResponseType(typeof(PixLimitAccountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PixLimitAccountResponseDto>> GetByAccountAsync(
        [FromRoute] string cpf,
        [FromRoute] string agencyNumber,
        [FromRoute] string accountNumber,
        CancellationToken cancellationToken)
    {
        var result = await pixLimitAccountService.GetByAccountAsync(
            cpf,
            agencyNumber,
            accountNumber,
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpPut("{cpf}/{agencyNumber}/{accountNumber}/limit")]
    [ProducesResponseType(typeof(PixLimitAccountResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PixLimitAccountResponseDto>> UpdateLimitAsync(
        [FromRoute] string cpf,
        [FromRoute] string agencyNumber,
        [FromRoute] string accountNumber,
        [FromBody] UpdatePixLimitAccountLimitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await pixLimitAccountService.UpdateLimitAsync(
            cpf,
            agencyNumber,
            accountNumber,
            new UpdatePixLimitAccountRequestDto(request.TransactionLimit),
            cancellationToken);

        return this.ToActionResult(result);
    }

    [HttpDelete("{cpf}/{agencyNumber}/{accountNumber}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] string cpf,
        [FromRoute] string agencyNumber,
        [FromRoute] string accountNumber,
        CancellationToken cancellationToken)
    {
        var result = await pixLimitAccountService.DeleteAsync(
            cpf,
            agencyNumber,
            accountNumber,
            cancellationToken);

        return this.ToActionResult(result);
    }
}
