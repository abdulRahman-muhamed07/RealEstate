using Microsoft.AspNetCore.Mvc;
using RealEstate.Application.Common;

namespace RealEstate.Api.Extensions;

public static class ApiResultExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result) =>
        result.Success
            ? controller.Ok(result.Data)
            : controller.StatusCode(result.ErrorCode switch
            {
                ErrorCode.Validation => StatusCodes.Status400BadRequest,
                ErrorCode.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorCode.Forbidden => StatusCodes.Status403Forbidden,
                ErrorCode.NotFound => StatusCodes.Status404NotFound,
                ErrorCode.Conflict => StatusCodes.Status409Conflict,
                ErrorCode.InvalidOperation => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError
            }, new { message = result.Error });
}
