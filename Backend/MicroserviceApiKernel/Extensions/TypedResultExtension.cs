using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MicroserviceApiKernel.Extensions;

public static class TypedResultExtension
{
    public static Results<TSuccess, BadRequest<string>, NotFound<string>, Conflict<string>, InternalServerError<string>, ProblemHttpResult> MapToTypedResult<TIn, TSuccess>(
        this Result<TIn> result,
        Func<TIn, TSuccess> onSuccess)
        where TSuccess : IResult
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value!);
        }

        return TypedError<TSuccess>(result.Error!);
    }

    public static Results<TSuccess, BadRequest<string>, NotFound<string>, Conflict<string>, InternalServerError<string>, ProblemHttpResult> MapToTypedResult<TSuccess>(
        this Result result,
        Func<TSuccess> onSuccess)
        where TSuccess : IResult
    {
        if (result.IsSuccess)
        {
            return onSuccess();
        }

        return TypedError<TSuccess>(result.Error!);
    }

    public static Results<TSuccess, BadRequest<string>, NotFound<string>, Conflict<string>, InternalServerError<string>, ProblemHttpResult> TypedError<TSuccess>(Error error)
        where TSuccess : IResult
    {
        return error.ErrorType switch
        {
            ErrorType.Internal => TypedResults.InternalServerError(error.ErrorName),
            ErrorType.NotFound => TypedResults.NotFound(error.ErrorName),
            ErrorType.Conflict => TypedResults.Conflict(error.ErrorName),
            ErrorType.Problem => TypedResults.Problem(error.ErrorName),
            ErrorType.Validation => TypedResults.BadRequest(error.ErrorName),
            _ => TypedResults.Problem(error.ErrorName, statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}