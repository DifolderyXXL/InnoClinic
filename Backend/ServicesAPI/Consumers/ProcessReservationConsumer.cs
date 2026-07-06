using Contracts.AppointmentContracts;
using FluentValidation;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using ServicesAPI.Application.Scheduling;
using ServicesAPI.Data;
using ServicesAPI.Endpoints.Services.DeleteService;
using ServicesAPI.Endpoints.Services.GetServices;

namespace ServicesAPI.Consumers;

public interface IScheduleService
{
    Task<Result<uint>> GetTimeStepByServiceIdAsync(long serviceId, CancellationToken ct);
}

public class ScheduleService(ServicesDbContext context) : IScheduleService
{
    public async Task<Result<uint>> GetTimeStepByServiceIdAsync(long serviceId, CancellationToken ct)
    {
        var service = await context.Services
            .Include(s => s.ServiceCategory)
            .FirstOrDefaultAsync(s => s.Id == serviceId, ct);

        if (service == null)
        {
            return ServiceErrors.ServiceNotFound();
        }

        return service.ServiceCategory.TimeSlotSize;
    }
}

public class ProcessReservationValidator : AbstractValidator<ProcessReservation>
{
    public ProcessReservationValidator()
    {
        RuleFor(x => x.StartSlotIndex).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ServiceId).GreaterThan(0);
    }
}
public class ProcessReservationConsumer(
    IReservationService reservationService, 
    IValidator<ProcessReservation> validator,
    IScheduleService scheduleService)
    : IConsumer<ProcessReservation>
{
    private async Task Fail(ConsumeContext<ProcessReservation> context)
    {
        await context.Publish(new ReservationFailed(context.Message.AppointmentId));
    }
    public async Task Consume(ConsumeContext<ProcessReservation> context)
    {
        var validation = await validator.ValidateAsync(context.Message, context.CancellationToken);
        if (!validation.IsValid)
        {
            await Fail(context);
            return;
        }

        var timeStepResult = await scheduleService.GetTimeStepByServiceIdAsync(context.Message.ServiceId, context.CancellationToken);
        if (timeStepResult.IsError)
        {
            await Fail(context);
            return;
        }

        var slotCount = timeStepResult.Value;
        
        var result = await reservationService.TryReserve(
            new(context.Message.Date, context.Message.StartSlotIndex, (int)slotCount),
            context.CancellationToken);

        if (result.IsSuccess)
        {
            await context.Publish(new TimeWindowReserved(context.Message.AppointmentId, result.ReservationId!.Value));   
        }
        else
        {
            await Fail(context);
        }
    }
}

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

public class CancelReservationConsumer(IReservationService reservationService) : IConsumer<CancelReservation>
{
    public async Task Consume(ConsumeContext<CancelReservation> context)
    {
        await reservationService.CancelReservation(context.Message.ReservationId, context.CancellationToken);
    }
}