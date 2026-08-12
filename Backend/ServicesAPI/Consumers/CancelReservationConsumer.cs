using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class CancelReservationConsumer(IReservationLifecycleManager reservationLifecycleManager) : IConsumer<CancelReservation>
{
    public async Task Consume(ConsumeContext<CancelReservation> context)
    {
        await reservationLifecycleManager.CancelAsync(context.Message.ReservationId, false, context.CancellationToken);
    }
}