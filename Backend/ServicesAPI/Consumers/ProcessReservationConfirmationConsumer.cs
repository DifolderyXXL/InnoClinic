using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ProcessReservationConfirmationConsumer(IReservationLifecycleManager reservationLifecycleManager, ILogger<ProcessReservationConfirmationConsumer> logger) : IConsumer<ProcessReservationConfirmation>
{
    public async Task Consume(ConsumeContext<ProcessReservationConfirmation> context)
    {
        var result = await reservationLifecycleManager.ConfirmAsync(context.Message.ReservationId, context.CancellationToken);

        if (result)
        {
            await context.Publish(new ReservationConfirmed(context.Message.AppointmentId, context.Message.ReservationId));   
        }
        else
        {
            await context.Publish(new ReservationFailed(context.Message.AppointmentId));   
        }
        
        logger.LogDebug("Reservation confirmation result {Result}: r: {Reservation}, a: {Appointment}", result, context.Message.ReservationId, context.Message.AppointmentId);
    }
}