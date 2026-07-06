using Contracts.AppointmentContracts;
using FluentValidation;
using MassTransit;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Consumers;

public class ProcessReservationValidator : AbstractValidator<ProcessReservation>
{
    public ProcessReservationValidator()
    {
        RuleFor(x => x.StartSlotIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SlotCount).GreaterThan(0);
    }
}
public class ProcessReservationConsumer(IScheduleService scheduleService, IValidator<ProcessReservation> validator) : IConsumer<ProcessReservation>
{
    public async Task Consume(ConsumeContext<ProcessReservation> context)
    {
        var validation = await validator.ValidateAsync(context.Message, context.CancellationToken);
        if (!validation.IsValid)
        {
            await context.Publish(new ReservationFailed(context.Message.AppointmentId));
            return;
        }
        
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

public class ProcessReservationConfirmationConsumer(IScheduleService scheduleService) : IConsumer<ProcessReservationConfirmation>
{
    public async Task Consume(ConsumeContext<ProcessReservationConfirmation> context)
    {
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

public class CancelReservationConsumer(IScheduleService scheduleService) : IConsumer<CancelReservation>
{
    public async Task Consume(ConsumeContext<CancelReservation> context)
    {
        await scheduleService.CancelSchedule(context.Message.ReservationId, context.CancellationToken);
    }
}