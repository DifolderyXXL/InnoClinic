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

        await publishEndpoint.Publish<UserAppointmentConfirmedIntegrationEvent>(new UserAppointmentConfirmedIntegrationEvent
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