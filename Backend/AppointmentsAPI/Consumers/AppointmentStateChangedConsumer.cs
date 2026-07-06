using AppointmentsAPI.Controllers;
using AppointmentsAPI.Data;
using Contracts.AppointmentContracts;
using MassTransit;

namespace AppointmentsAPI.Consumers;

public class AppointmentStateChangedConsumer(IAppointmentService service, ILogger<AppointmentStateChangedConsumer>? logger) : IConsumer<AppointmentSagaStateChanged>
{
    public async Task Consume(ConsumeContext<AppointmentSagaStateChanged> context)
    {
        try
        {
            await service.UpdateState(context.Message.AppointmentId, context.Message.State, context.CancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Update state failed for appointment: {AppointmentId}", context.Message.AppointmentId);
        }
    }
}

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