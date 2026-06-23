using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.User.GetProfiles;
namespace ProfilesAPI.Endpoints.User;

public class Endpoint : IEndpoint
{
    public record Response(PatientDto Patient, DoctorDto Doctor, ReceptionistDto Receptionist);

    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/get-profiles", async (
            UserClaimInfo user,
            IQueryHandler<GetUserProfileQuery, GetUserProfileQueryResponse> handler,
            ProfilesDbContext context,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);

            var result = await handler.Handle(new(guid, user.Roles), ct);

            return result.MapTyped(x => TypedResults.Ok(new Response(x.Patient, x.Doctor, x.Receptionist)));
        })
        .RequireAuthorization(RolePolicy.Client)
        .WithDescription("Provides user all available profiles.")
        .Produces<Response>(StatusCodes.Status200OK);
    }
}

public static class ConstantRoles
{
    public const string Patient = "Client";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
}

public static class TypedResultExtension
{
    public static TResult Map<T, TResult>(this Result<T> result, Func<Result<T>, TResult> onFailure, Func<Result<T>, TResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess(result) : onFailure(result);
    }

    public static IResult MapTyped<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        return result.IsSuccess ? onSuccess(result.Value!) : TypedError(result.Error!);
    }

    public static IResult TypedError(Error error)
    {
        return error.ErrorType switch
        {
            ErrorType.Internal => TypedResults.InternalServerError(error.ErrorName),
            ErrorType.NotFound => TypedResults.NotFound(error.ErrorName),
            ErrorType.Conflict => TypedResults.Conflict(error.ErrorName),
            ErrorType.Problem => TypedResults.Problem(error.ErrorName),
            ErrorType.Validation => TypedResults.Problem(error.ErrorName),
            _ => throw new NotImplementedException(),
        };
    }
}