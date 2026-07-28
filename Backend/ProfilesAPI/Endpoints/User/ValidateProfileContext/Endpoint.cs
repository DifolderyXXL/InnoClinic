using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Doctors.GetDoctorById;
using ProfilesAPI.Endpoints.Patients.Create;

namespace ProfilesAPI.Endpoints.User.ValidateProfileContext;

public class ValidateProfilesContextEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("profiles/validate-profile", async(
            [FromBody] ValidateProfilesCommand command,
            ICommandHandler<ValidateProfilesCommand, ValidateProfilesResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(command, ct);
            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.IdentityServer);
    }
}

public record ValidateProfilesCommand(Guid DoctorId, Guid PatientId, string OfficeId) : ICommand<ValidateProfilesResponse>;

public record ValidateProfilesResponse(
    bool IsValid, 
    string DoctorFullName, 
    string PatientFullName, 
    long DoctorSpecializationId);

public class ValidateProfilesCommandHandler(ProfilesDbContext context) 
    : ICommandHandler<ValidateProfilesCommand, ValidateProfilesResponse>
{
    public async Task<Result<ValidateProfilesResponse>> Handle(ValidateProfilesCommand command, CancellationToken ct)
    {
        var doctor = await context.Doctors
            .Include(d=>d.Account)
            .FirstOrDefaultAsync(d => d.AccountId == command.DoctorId && d.OfficeId == command.OfficeId, ct);

        if (doctor == null)
            return DoctorErrors.DoctorNotFoundInOffice();

        var patient = await context.Patients
            .Include(d=>d.Account)
            .FirstOrDefaultAsync(p => p.AccountId == command.PatientId, ct);

        if (patient == null)
            return PatientErrors.NotFound();

        return new ValidateProfilesResponse(
            IsValid: true,
            DoctorFullName: $"{doctor.Account.LastName} {doctor.Account.FirstName}",
            PatientFullName: $"{patient.Account.LastName} {patient.Account.FirstName}",
            DoctorSpecializationId: doctor.SpecializationId
        );
    }
}