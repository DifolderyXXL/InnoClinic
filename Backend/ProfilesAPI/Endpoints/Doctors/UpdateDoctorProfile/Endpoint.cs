using FluentValidation;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.CustomBindAsync;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Doctors.GetDoctorById;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Doctors.UpdateDoctorProfile;

public class UpdateDoctorProfileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/doctor/{id:long}", async (
            long id,
            UpdateDoctorProfileCommand request,
            ICommandHandler<UpdateDoctorProfileCommand> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request with { Id = id }, ct);
        }).RequireAuthorization(RolePolicy.Receptionist);

        builder.MapPut("/my-doctor", async (
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

            return result.MapToTypedResult(() => TypedResults.Ok());
        }).RequireAuthorization(RolePolicy.Doctor);
    }
}

public record UpdateDoctorProfileCommand(
    long Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    long CareerStartYear,
    long SpecializationId,
    Status Status,
    long OfficeId) : ICommand;

public class UpdateDoctorProfileCommandHandler(ProfilesDbContext context) : ICommandHandler<UpdateDoctorProfileCommand>
{
    public async Task<Result> Handle(UpdateDoctorProfileCommand command, CancellationToken ct)
    {
        var doctor = await context.Doctors
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.Id == command.Id);

        if (doctor == null) return DoctorErrors.DoctorNotFound();

        var specialization = await context.Specializations.FindAsync([command.SpecializationId], ct);
        if (specialization == null)
        {
            return SpecializationErrors.SpecializationNotFound();
        }

        doctor.Account.FirstName = command.FirstName;
        doctor.Account.LastName = command.LastName;
        doctor.Account.MiddleName = command.MiddleName;

        doctor.DateOfBirth = command.DateOfBirth;
        doctor.OfficeId = command.OfficeId;
        doctor.Specialization = specialization;

        doctor.CareerStartYear = command.CareerStartYear;
        doctor.Status = command.Status;

        await context.SaveChangesAsync(ct);

        return Result.Success();
    }
}

public class UpdateDoctorProfileCommandValidator : AbstractValidator<UpdateDoctorProfileCommand>
{
    public UpdateDoctorProfileCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
    }
}