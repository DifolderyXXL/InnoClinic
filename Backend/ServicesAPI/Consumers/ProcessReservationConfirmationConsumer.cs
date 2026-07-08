using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ProcessReservationConfirmationConsumer(IReservationService reservationService) : IConsumer<ProcessReservationConfirmation>
{
    public async Task Consume(ConsumeContext<ProcessReservationConfirmation> context)
    {
        var result = await reservationService.TryConfirmReservation(context.Message.ReservationId, context.CancellationToken);

        if (result)
        {
            await context.Publish(new ReservationConfirmed(context.Message.AppointmentId, context.Message.ReservationId));   
        }
        else
        {
            await context.Publish(new ReservationFailed(context.Message.AppointmentId));   
        }
    }
}