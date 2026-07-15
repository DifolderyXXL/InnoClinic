using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.CQRS;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.User.GetProfiles;

namespace ProfilesAPI.Endpoints.User;

public class Endpoint : IEndpoint
{
    public record Response(BaseAccountDto Account, PatientDto Patient, DoctorDto Doctor, ReceptionistDto Receptionist);

    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/profiles/me", async (
            UserClaimInfo user,
            IQueryHandler<GetUserProfileQuery, GetUserProfileQueryResponse> handler,
            CancellationToken ct) =>
        {
            var guid = Guid.Parse(user.Id);

            var result = await handler.Handle(new(guid, user.Roles), ct);

            return result.MapToTypedResult(x => TypedResults.Ok(new Response(x.Account, x.Patient, x.Doctor, x.Receptionist)));
        })
        .RequireAuthorization(RolePolicy.Client)
        .WithDescription("Provides user all available profiles.")
        .Produces<Response>(StatusCodes.Status200OK);
    }
}

public static class ConstantRoles
{
    public const string Patient = "client";
    public const string Doctor = "doctor";
    public const string Receptionist = "receptionist";
}