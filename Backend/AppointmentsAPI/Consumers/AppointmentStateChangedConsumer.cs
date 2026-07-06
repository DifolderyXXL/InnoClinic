using AppointmentsAPI.Controllers;
using AppointmentsAPI.Data;
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