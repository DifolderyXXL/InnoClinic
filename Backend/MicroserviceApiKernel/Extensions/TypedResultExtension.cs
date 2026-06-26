using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace MicroserviceApiKernel.Extensions;

public static class TypedResultExtension
{
    public static Results<TSuccess, ValidationProblem, NotFound<string>, Conflict<string>, InternalServerError<string>, ProblemHttpResult> MapToTypedResult<TIn, TSuccess>(
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

    public static Results<TSuccess, ValidationProblem, NotFound<string>, Conflict<string>, InternalServerError<string>, ProblemHttpResult> MapToTypedResult<TSuccess>(
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

    public static Results<TSuccess, ValidationProblem, NotFound<string>, Conflict<string>, InternalServerError<string>, ProblemHttpResult> TypedError<TSuccess>(Error error)
        where TSuccess : IResult
    {
        return error.ErrorType switch
        {
            ErrorType.Internal => TypedResults.InternalServerError(error.ToString()),
            ErrorType.NotFound => TypedResults.NotFound(error.ToString()),
            ErrorType.Conflict => TypedResults.Conflict(error.ToString()),
            ErrorType.Problem => TypedResults.Problem(error.ToString()),
            ErrorType.Validation => TypedResults.ValidationProblem(error.ValidationResults ?? Enumerable.Empty<KeyValuePair<string, string[]>>(), error.ToString()),
            _ => TypedResults.Problem(error.ToString(), statusCode: StatusCodes.Status500InternalServerError)
        };
    }
}