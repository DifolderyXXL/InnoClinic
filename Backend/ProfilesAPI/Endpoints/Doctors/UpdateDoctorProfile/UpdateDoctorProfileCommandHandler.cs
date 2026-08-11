using Contracts.ProfilesContracts;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Data;
using ProfilesAPI.Endpoints.Doctors.GetDoctorById;
using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.Doctors.UpdateDoctorProfile;

public record UpdateDoctorProfileCommand(
    long Id,
    DateOnly DateOfBirth,
    long CareerStartYear,
    long SpecializationId,
    Status Status,
    string OfficeId) : ICommand;
public class UpdateDoctorProfileCommandHandler(ProfilesDbContext context, IPublishEndpoint publishEndpoint) : ICommandHandler<UpdateDoctorProfileCommand>
{
    public async Task<Result> Handle(UpdateDoctorProfileCommand command, CancellationToken ct)
    {
        var doctor = await context.Doctors
            .Include(x => x.Account)
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken: ct);

        if (doctor == null) return DoctorErrors.DoctorNotFound();

        var specialization = await context.Specializations.FindAsync([command.SpecializationId], ct);
        if (specialization == null)
        {
            return SpecializationErrors.SpecializationNotFound();
        }
        
        doctor.DateOfBirth = command.DateOfBirth;
        doctor.OfficeId = command.OfficeId;
        doctor.Specialization = specialization;

        doctor.CareerStartYear = command.CareerStartYear;
        doctor.Status = command.Status;

        await publishEndpoint.Publish<DoctorUpdatedEvent>(new()
        {
            AccountId = doctor.Account.Id,
            Id = doctor.Id,
            CareerStartYear = doctor.CareerStartYear,
            DateOfBirth = doctor.DateOfBirth,
            OfficeId = doctor.OfficeId,
        }, ct);
        
        await context.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}