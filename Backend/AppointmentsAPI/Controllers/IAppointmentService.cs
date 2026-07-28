using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public interface IAppointmentService
{
    public Task<Result<Guid>> AddAppointment(Appointment appointment, CancellationToken ct);
    public Task<Result> UpdateState(Guid appointmentId, AppointmentState state, CancellationToken ct);
    public Task<Result> UpdateReservation(Guid appointmentId, long reservationId, TimeSpan beginTime, TimeSpan endTime, CancellationToken ct);
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
        var rowsAffected = await context.Appointments
            .Where(a => a.Id == appointmentId)
            .ExecuteUpdateAsync(s => s.SetProperty(a => a.State, state), ct);

        if (rowsAffected == 0)
        {
            return AppointmentErrors.AppointmentNotFound();
        }

        return Result.Success();
    }

    public async Task<Result> UpdateReservation(Guid appointmentId, long reservationId, TimeSpan beginTime, TimeSpan endTime,  CancellationToken ct)
    {
        var rowsAffected = await context.Appointments
            .Where(a => a.Id == appointmentId && a.State != AppointmentState.Failed)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.ReservationId, reservationId)
                    .SetProperty(a => a.BeginTime, beginTime)
                    .SetProperty(a => a.EndTime, endTime), 
                ct);

        if (rowsAffected == 0)
        {
            return AppointmentErrors.AppointmentNotFoundOrInvalidState();
        }
        
        return Result.Success();
    }
}

public static class AppointmentErrors
{
    public static Error AppointmentNotFound() => Error.Create(ErrorType.NotFound);
    public static Error AppointmentNotFoundOrInvalidState() => Error.Create(ErrorType.Validation);
}