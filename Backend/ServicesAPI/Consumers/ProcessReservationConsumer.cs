using Contracts.AppointmentContracts;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ProcessReservationConsumer : IConsumer<ProcessReservation>
{
    public async Task Consume(ConsumeContext<ProcessReservation> context)
    {
        var scheduleService = context.GetPayload<IScheduleService>();
        var result = await scheduleService.TrySchedule(
            new(context.Message.Date, context.Message.StartSlotIndex, context.Message.SlotCount),
            context.CancellationToken);

        if (result.IsSuccess)
        {
            await context.Publish(new TimeWindowReserved(context.Message.AppointmentId, result.ReservationId!.Value));   
        }
        else
        {
            await context.Publish(new ReservationFailed(context.Message.AppointmentId));
        }
    }
}

public class ProcessReservationConfirmationConsumer : IConsumer<ProcessReservationConfirmation>
{
    public async Task Consume(ConsumeContext<ProcessReservationConfirmation> context)
    {
        var scheduleService = context.GetPayload<IScheduleService>();
        var result = await scheduleService.TryConfirmSchedule(context.Message.ReservationId, context.CancellationToken);

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

public class CancelReservationConsumer : IConsumer<CancelReservation>
{
    public async Task Consume(ConsumeContext<CancelReservation> context)
    {
        var scheduleService = context.GetPayload<IScheduleService>();

        await scheduleService.CancelSchedule(context.Message.ReservationId, context.CancellationToken);
    }
}