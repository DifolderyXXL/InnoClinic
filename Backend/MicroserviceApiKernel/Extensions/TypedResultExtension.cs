using System.Net;
using System.Net.Http.Json;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

public static class HttpClientErrorExtensions
{
    public static async Task<Error> ReadErrorAsync(this HttpResponseMessage response, CancellationToken ct)
    {
        var errorType = response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => ErrorType.Validation,
            System.Net.HttpStatusCode.NotFound => ErrorType.NotFound,
            System.Net.HttpStatusCode.Conflict => ErrorType.Conflict,
            _ => ErrorType.Problem
        };
        
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>(cancellationToken: ct);

            if (problem != null)
            {
                string message = !string.IsNullOrWhiteSpace(problem.Detail) 
                    ? problem.Detail 
                    : problem.Title ?? "Validation error occurred";

                var validationResults = problem.Errors?.ToDictionary(
                    k => k.Key, 
                    v => v.Value);
                

                return new Error(
                    errorName: $"Http.{(int)response.StatusCode}",
                    errorDescription: message, 
                    errorType: errorType,
                    validationResults: validationResults
                );
            }
        }
        catch
        {
        }

        var rawText = await response.Content.ReadAsStringAsync(ct);
        return new Error(
            errorName: $"Http.{(int)response.StatusCode}",
            errorDescription: !string.IsNullOrWhiteSpace(rawText) ? rawText : response.ReasonPhrase ?? "HTTP Error",
            errorType: errorType
        );
    }
}