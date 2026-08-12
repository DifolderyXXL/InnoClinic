using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ReservationExpiredConsumer(IReservationLifecycleManager reservationLifecycleManager, ILogger<ReservationExpiredConsumer> logger) : IConsumer<ReservationExpired>
{
    public async Task Consume(ConsumeContext<ReservationExpired> context)
    {
        await reservationLifecycleManager.CancelAsync(context.Message.ReservationId, true, context.CancellationToken);
        logger.LogDebug("Reservation expired: r: {Reservation}, a: {Appointment}", context.Message.ReservationId, context.Message.AppointmentId);
    }
}