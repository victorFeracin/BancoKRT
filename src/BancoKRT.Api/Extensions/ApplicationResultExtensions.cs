using BancoKRT.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace BancoKRT.Api.Extensions;

public static class ApplicationResultExtensions
{
    public static ActionResult ToActionResult(
        this ControllerBase controller,
        ApplicationResult result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return controller.ToErrorResult(result.Error!);
    }

    public static ActionResult<T> ToActionResult<T>(
        this ControllerBase controller,
        ApplicationResult<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return controller.ToErrorResult(result.Error!);
    }

    public static ActionResult<T> ToCreatedAtRouteResult<T>(
        this ControllerBase controller,
        ApplicationResult<T> result,
        string routeName,
        object routeValues)
    {
        if (result.IsSuccess)
        {
            return controller.CreatedAtRoute(routeName, routeValues, result.Value);
        }

        return controller.ToErrorResult(result.Error!);
    }

    private static ActionResult ToErrorResult(this ControllerBase controller, ApplicationError error)
    {
        var problemDetails = new ProblemDetails
        {
            Title = GetTitle(error.Type),
            Detail = error.Message,
            Status = GetStatusCode(error.Type)
        };

        return controller.StatusCode(problemDetails.Status.Value, problemDetails);
    }

    private static int GetStatusCode(ApplicationErrorType errorType)
    {
        return errorType switch
        {
            ApplicationErrorType.Validation => StatusCodes.Status400BadRequest,
            ApplicationErrorType.Conflict => StatusCodes.Status409Conflict,
            ApplicationErrorType.NotFound => StatusCodes.Status404NotFound,
            ApplicationErrorType.BusinessRule => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(ApplicationErrorType errorType)
    {
        return errorType switch
        {
            ApplicationErrorType.Validation => "Erro de validação",
            ApplicationErrorType.Conflict => "Conflito de recurso",
            ApplicationErrorType.NotFound => "Recurso não encontrado",
            ApplicationErrorType.BusinessRule => "Ação não permitida",
            _ => "Erro na aplicação"
        };
    }
}
