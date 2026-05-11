using BancoKRT.Api.Contracts;
using BancoKRT.Api.Extensions;
using BancoKRT.Application.DTOs;
using BancoKRT.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.Api.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public sealed class PixTransactionsController(IPixLimitAccountService pixLimitAccountService) : ControllerBase
{
    [HttpPost("process")]
    [ProducesResponseType(typeof(ProcessPixTransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProcessPixTransactionResponseDto>> ProcessAsync(
        [FromBody] ProcessPixTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await pixLimitAccountService.ProcessTransactionAsync(
            new ProcessPixTransactionRequestDto(
                request.Cpf,
                request.AgencyNumber,
                request.AccountNumber,
                request.Amount),
            cancellationToken);

        return this.ToActionResult(result);
    }
}
