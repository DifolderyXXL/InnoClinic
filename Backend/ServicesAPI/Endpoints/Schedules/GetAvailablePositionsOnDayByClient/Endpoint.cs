using MicroserviceApiKernel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProfilesAPI.CustomBindAsync;
using ServicesAPI.Application.Scheduling;

namespace ServicesAPI.Endpoints.Schedules.GetAvailablePositionsOnDayByClient;

public class GetAvailablePositionsOnDayEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/schedules/doctor/{doctorId:guid}", 
            async Task<Results<BadRequest<string>, Ok<AvailablePositionsOnDayResponse>>> (
                [FromRoute] Guid doctorId,
                [FromQuery] DateOnly dateOnly,
                UserClaimInfo userInfo,
                IOptions<ScheduleOptions> options,
                IReservationService service,
                CancellationToken ct) =>
            {
                var guid = Guid.Parse(userInfo.Id);
                var positions = await service.GetAvailablePositionsOnDay(doctorId, guid, dateOnly, ct);

                var timeWindows = positions.Select(x => new AvailableTimeWindowDto(
                    x.TimeSlotStart,
                    x.TimeSlotSize,
                    options.Value.GetSlotTime(x.TimeSlotStart),
                    options.Value.GetSlotTime(x.TimeSlotStart + x.TimeSlotSize)
                )).ToArray();
                
                return TypedResults.Ok(new AvailablePositionsOnDayResponse(
                    options.Value.WorkScheduleBeginTime,
                    options.Value.WorkScheduleEndTime,
                    options.Value.TimeSlotLength,
                    options.Value.GetSlotsAmount(),
                    timeWindows
                    ));
            }).RequireAuthorization();
    }

    public record AvailablePositionsOnDayResponse(
        TimeSpan DayBeginTime, TimeSpan DayEndTime, TimeSpan TimeSlotLength, int SlotAmount, AvailableTimeWindowDto[] AvailableTimeWindows);
    public record AvailableTimeWindowDto(int TimeSlotStart, int TimeSlotSize, TimeSpan BeginTime, TimeSpan EndTime);
}