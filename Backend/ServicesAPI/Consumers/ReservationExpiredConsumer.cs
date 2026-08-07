using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ReservationExpiredConsumer(IReservationService reservationService, ILogger<ReservationExpiredConsumer> logger) : IConsumer<ReservationExpired>
{
    public async Task Consume(ConsumeContext<ReservationExpired> context)
    {
        await reservationService.CancelReservation(context.Message.ReservationId, true, context.CancellationToken);
        logger.LogDebug("Reservation expired: r: {Reservation}, a: {Appointment}", context.Message.ReservationId, context.Message.AppointmentId);
    }
}