using Contracts.AppointmentContracts;
using FluentValidation;
using MassTransit;
using Microsoft.Extensions.Options;
using ServicesAPI.Application.Scheduling;
using ServicesAPI.Data;

namespace ServicesAPI.Consumers;

public class ProcessRescheduleReservationValidator : AbstractValidator<ProcessRescheduleReservation>
{
    public ProcessRescheduleReservationValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.CurrentReservationId).GreaterThan(0);
        RuleFor(x => x.NewStartSlotIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ServiceId).GreaterThan(0);
    }
}

public class ProcessRescheduleReservationConsumer(
    IReservationLifecycleManager lifecycleManager,
    IValidator<ProcessRescheduleReservation> validator,
    IScheduleService scheduleService,
    ServicesDbContext db,
    ILogger<ProcessRescheduleReservationConsumer> logger,
    IOptions<ScheduleOptions> scheduleOptions,
    IOptions<ReservationOptions> reservationOptions)
    : IConsumer<ProcessRescheduleReservation>
{
    private async Task Fail(ConsumeContext<ProcessRescheduleReservation> context, string reason)
    {
        await context.Publish(new AppointmentRescheduleFailed(
            context.Message.AppointmentId, 
            reason));
    }

    public async Task Consume(ConsumeContext<ProcessRescheduleReservation> context)
    {
        var validation = await validator.ValidateAsync(context.Message, context.CancellationToken);
        if (!validation.IsValid)
        {
            await Fail(context, "Invalid reschedule command parameters.");
            return;
        }

        var timeStepResult = await scheduleService.GetTimeStepByServiceIdAsync(context.Message.ServiceId, context.CancellationToken);
        if (timeStepResult.IsError)
        {
            await Fail(context, "Service or category not found.");
            return;
        }

        var slotCount = (int)timeStepResult.Value;

        var rescheduleResult = await lifecycleManager.RescheduleAsync(
            context.Message.CurrentReservationId,
            context.Message.NewDate,
            context.Message.NewStartSlotIndex,
            context.CancellationToken);

        if (!rescheduleResult)
        {
            await Fail(context, "The selected time slot is no longer available.");
            return;
        }

        var newReservationId = context.Message.CurrentReservationId;
        var newEndSlotIndex = context.Message.NewStartSlotIndex + slotCount;

        await context.Publish(new AppointmentRescheduled(
            context.Message.AppointmentId,
            newReservationId,
            context.Message.NewDate,
            context.Message.NewStartSlotIndex,
            scheduleOptions.Value.GetSlotTime(context.Message.NewStartSlotIndex),
            scheduleOptions.Value.GetSlotTime(newEndSlotIndex)
        ));
        
        await db.SaveChangesAsync(context.CancellationToken);
    }
}

public class ProcessRescheduleReservationFaultConsumer : IConsumer<Fault<ProcessRescheduleReservation>>
{
    public async Task Consume(ConsumeContext<Fault<ProcessRescheduleReservation>> context)
    {
        var originalMessage = context.Message.Message;
        
        await context.Publish(new AppointmentRescheduleFailed(
            originalMessage.AppointmentId, 
            "Internal server error occurred during rescheduling."));
    }
}