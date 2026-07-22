using MicroserviceApiKernel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Endpoints.Schedules.GetAvailablePositionsOnDayByClient;

public class GetAvailablePositionsOnDayEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/schedules/doctor/{doctorId:guid}", 
            async Task<Results<BadRequest<string>, Ok<AvailableTimeWindowDto[]>>> (
                [FromRoute] Guid doctorId,
                [FromQuery] DateOnly dateOnly,
                IOptions<ScheduleOptions> options,
                IReservationService service,
                CancellationToken ct) =>
        {
            var positions = await service.GetAvailablePositionsOnDay(doctorId, dateOnly, ct);

            return TypedResults.Ok(positions.Select(x=>new AvailableTimeWindowDto(
                x.TimeSlotStart,
                x.TimeSlotSize,
                options.Value.GetSlotTime(x.TimeSlotStart),
                options.Value.GetSlotTime(x.TimeSlotStart + x.TimeSlotSize)
                )).ToArray());
        });
    }

    public record AvailableTimeWindowDto(int TimeSlotStart, int TimeSlotSize, TimeSpan BeginTime, TimeSpan EndTime);
}