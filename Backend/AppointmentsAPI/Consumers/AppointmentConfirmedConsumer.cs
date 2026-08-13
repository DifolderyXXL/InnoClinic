using AppointmentsAPI.Data;
using Contracts.AppointmentContracts;
using Contracts.Notifications;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AppointmentsAPI.Consumers;

public class AppointmentConfirmedConsumer(
    AppointmentDbContext db, 
    IPublishEndpoint publishEndpoint) : IConsumer<AppointmentConfirmedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<AppointmentConfirmedIntegrationEvent> context)
    {
        var appointment = await db.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == context.Message.AppointmentId);

        if (appointment == null) return;

        await publishEndpoint.Publish(new UserAppointmentConfirmedIntegrationEvent
        {
            PatientEmail = appointment.PatientEmail,
            PatientName = appointment.PatientFullName,
            DoctorName = appointment.DoctorFullName,
            ServiceName = appointment.ServiceName,
            SpecializationName = appointment.SpecializationName,
            CategoryName = appointment.CategoryName,
            Date = appointment.Date,
            BeginTime = appointment.BeginTime!.Value,
            EndTime = appointment.EndTime!.Value
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}


public class AppointmentRescheduledConsumer(
    AppointmentDbContext db) : IConsumer<AppointmentRescheduled>
{
    public async Task Consume(ConsumeContext<AppointmentRescheduled> context)
    {
        var message = context.Message;

        await db.Appointments
            .Where(a => a.Id == message.AppointmentId)
            .ExecuteUpdateAsync(setters => setters
                    .SetProperty(a => a.ReservationId, message.NewReservationId)
                    .SetProperty(a => a.Date, message.NewDate)
                    .SetProperty(a => a.StartSlotIndex, message.NewStartSlotIndex)
                    .SetProperty(a => a.BeginTime, message.NewBeginTime)
                    .SetProperty(a => a.EndTime, message.NewEndTime),
                context.CancellationToken);
    }
}