using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using MicroserviceApiKernel.Results;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public interface IAppointmentService
{
    public Task<Result<Guid>> AddAppointment(Appointment appointment, CancellationToken ct);
    public Task<Result> UpdateState(Guid appointmentId, AppointmentState state, CancellationToken ct);
}
public class AppointmentService(AppointmentDbContext context) : IAppointmentService
{
    public async Task<Result<Guid>> AddAppointment(Appointment appointment, CancellationToken ct)
    {
        await context.Appointments.AddAsync(appointment, ct);
        await context.SaveChangesAsync(ct);

        return appointment.Id;
    }

    public async Task<Result> UpdateState(Guid appointmentId, AppointmentState state, CancellationToken ct)
    {
        var appointment = await context.Appointments.FindAsync([appointmentId], ct);

        if (appointment == null)
        {
            return AppointmentErrors.AppointmentNotFound();
        }
        
        appointment.State = state;
        
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public static class AppointmentErrors
{
    public static Error AppointmentNotFound() => Error.Create(ErrorType.NotFound);
}