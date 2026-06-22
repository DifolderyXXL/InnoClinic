using MicroserviceApiKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.User;

public record BaseAccountDto(
    Guid Id,
    string Email,
    string? PhoneNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    long? PhotoId
);

public record PatientDto(
    long Id,
    DateOnly DateOfBirth,
    BaseAccountDto Account
);

public record DoctorDto(
    long Id,
    DateOnly DateOfBirth,
    long OfficeId,
    long CareerStartYear,
    string SpecializationName,
    BaseAccountDto Account
);

public record ReceptionistDto(
    long Id,
    long OfficeId,
    BaseAccountDto Account
);
public class GetProfiles : IEndpoint
{
    public record Response(PatientDto Patient, DoctorDto Doctor, ReceptionistDto Receptionist);

    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/get-profiles", async (
            UserClaimInfo user,
            ProfilesDbContext context) =>
        {
            var guid = Guid.Parse(user.Id);

            var account = await context.Accounts
                .FirstOrDefaultAsync(x => x.Id == guid);

            if (account == null) return Results.BadRequest();

            var baseAccount = account.ToDto();

            var response = new Response(
                    account.Patient.ToDto(baseAccount),
                    user.Roles.Any(x => x == ConstantRoles.Doctor) ? account.Doctor.ToDto(baseAccount) : null,
                    user.Roles.Any(x => x == ConstantRoles.Receptionist) ? account.Receptionist.ToDto(baseAccount) : null
                    );

            return Results.Ok(response);
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

public static class DtoConverters
{
    public static BaseAccountDto ToDto(this Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new BaseAccountDto(
            account.Id,
            account.Email,
            account.PhoneNumber,
            account.FirstName,
            account.LastName,
            account.MiddleName,
            account.PhotoId
        );
    }

    public static PatientDto? ToDto(this Models.Patient? patient, BaseAccountDto baseAccount)
    {
        if (patient == null) return null;

        return new PatientDto(
            patient.Id,
            patient.DateOfBirth,
            baseAccount
        );
    }

    public static DoctorDto? ToDto(this Doctor? doctor, BaseAccountDto baseAccount)
    {
        if (doctor == null) return null;

        return new DoctorDto(
            doctor.Id,
            doctor.DateOfBirth,
            doctor.OfficeId,
            doctor.CareerStartYear,
            doctor.Specialization?.SpecializationName ?? "Unknown",
            baseAccount
        );
    }

    public static ReceptionistDto? ToDto(this Receptionist? receptionist, BaseAccountDto baseAccount)
    {
        if (receptionist == null) return null;

        return new ReceptionistDto(
            receptionist.Id,
            receptionist.OfficeId,
            baseAccount
        );
    }
}