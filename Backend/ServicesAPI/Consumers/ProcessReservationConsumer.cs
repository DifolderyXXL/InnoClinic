using System.ComponentModel.DataAnnotations;
using Contracts.AppointmentContracts;
using FluentValidation;
using MassTransit;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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

public class ReservationOptions
{
    public const string SectionName = "Reservation";
    
    [Required]
    public TimeSpan ReserveTime { get; set; }
}
public class ProcessReservationConsumer(
    IReservationService reservationService, 
    IValidator<ProcessReservation> validator,
    IScheduleService scheduleService,
    IOptions<ReservationOptions> reservationOptions)
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
            await context.SchedulePublish(
                delay: reservationOptions.Value.ReserveTime,
                message: new ReservationExpired(context.Message.AppointmentId, result.ReservationId!.Value));   
        }
        else
        {
            await Fail(context);
        }
    }
}