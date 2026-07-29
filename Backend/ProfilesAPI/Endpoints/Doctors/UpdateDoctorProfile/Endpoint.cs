using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Doctors.UpdateDoctorProfile;

public class UpdateDoctorProfileEndpoint : IDoctorEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/doctors/{id:long}", async (
            long id,
            UpdateDoctorProfileCommand request,
            ICommandHandler<UpdateDoctorProfileCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Doctors.Manage);

        builder.MapPut("/doctors/me", async (
            UpdateDoctorProfileCommand request,
            UserClaimInfo user,
            ProfilesDbContext context,
            ICommandHandler<UpdateDoctorProfileCommand> handler,
            CancellationToken ct) =>
        {
            var accountId = Guid.Parse(user.Id);

            long doctorId = await context.Doctors
                .Where(x => x.AccountId == accountId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(ct);

            if (doctorId == 0)
            {
                return TypedResults.NotFound("No doctor profile associated with this account.");
            }

            var result = await handler.Handle(request with { Id = doctorId }, ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).HasPermissions(Permissions.Doctors.Read);
    }
}