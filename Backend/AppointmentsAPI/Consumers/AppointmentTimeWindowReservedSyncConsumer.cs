using AppointmentsAPI.Controllers;
using Contracts.AppointmentContracts;
using MassTransit;

namespace AppointmentsAPI.Consumers;

public class AppointmentTimeWindowReservedSyncConsumer(
    IAppointmentService service, 
    ILogger<AppointmentTimeWindowReservedSyncConsumer>? logger) : IConsumer<TimeWindowReserved>
{
    public async Task Consume(ConsumeContext<TimeWindowReserved> context)
    {
        try
        {
            await service.UpdateReservationId(context.Message.AppointmentId, context.Message.ReservationId, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Sync reservation id failed for appointment: {AppointmentId}", context.Message.AppointmentId);
        }
    }
}