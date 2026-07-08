using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ReservationExpiredConsumer(IReservationService reservationService) : IConsumer<ReservationExpired>
{
    public async Task Consume(ConsumeContext<ReservationExpired> context)
    {
        await reservationService.CancelReservation(context.Message.ReservationId, context.CancellationToken);
    }
}