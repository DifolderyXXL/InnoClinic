using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class CancelReservationConsumer(IReservationService reservationService) : IConsumer<CancelReservation>
{
    public async Task Consume(ConsumeContext<CancelReservation> context)
    {
        await reservationService.CancelReservation(context.Message.ReservationId, context.CancellationToken);
    }
}