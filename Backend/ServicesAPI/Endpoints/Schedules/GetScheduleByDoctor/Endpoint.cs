using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProfilesAPI.CustomBindAsync;
using ServicesAPI.Application.Scheduling;
using ServicesAPI.Data;

namespace ServicesAPI.Endpoints.Schedules.GetScheduleByDoctor;

public class GetScheduleByDoctorEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("schedules/today/me", async (
            IQueryHandler<GetScheduleQuery, GetScheduleResponse> handler,
            UserClaimInfo user,
            CancellationToken ct) =>
        {
            var id = Guid.Parse(user.Id);
            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            
            var result = await handler.Handle(new(date, id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Doctor);

        builder.MapGet("schedules/me", async(
            [FromQuery] DateOnly date,
            IQueryHandler<GetScheduleQuery, GetScheduleResponse> handler,
            UserClaimInfo user,
            CancellationToken ct) =>
        {
            var id = Guid.Parse(user.Id);
            
            var result = await handler.Handle(new(date, id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Doctor);
        
        builder.MapGet("schedules/{id:guid}", async(
            [FromRoute] Guid id,
            [FromQuery] DateOnly date,
            IQueryHandler<GetScheduleQuery, GetScheduleResponse> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new(date, id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Receptionist);
        
                
        builder.MapGet("schedules/today/{id:guid}", async(
            [FromRoute] Guid id,
            IQueryHandler<GetScheduleQuery, GetScheduleResponse> handler,
            CancellationToken ct) =>
        {
            var date = DateOnly.FromDateTime(DateTime.UtcNow);
            var result = await handler.Handle(new(date, id), ct);

            return result.MapToTypedResult(TypedResults.Ok);
        }).RequireAuthorization(RolePolicy.Receptionist);
    }
}

public record GetScheduleQuery(DateOnly Date, Guid? DoctorId) : IQuery<GetScheduleResponse>;

public record GetScheduleResponse(List<ScheduleDto> Schedule);

public class GetScheduleQueryHandler(ServicesDbContext context, IOptions<ScheduleOptions> options) : IQueryHandler<GetScheduleQuery, GetScheduleResponse>
{
    public async Task<Result<GetScheduleResponse>> Handle(GetScheduleQuery query, CancellationToken ct)
    {
        var reservation = context.ReservedTimeWindows
            .Where(x => x.Date == query.Date);

        if (query.DoctorId != null)
        {
            reservation = reservation.Where(x=>x.DoctorId == query.DoctorId);
        }

        var slots = await reservation
            .Where(x=>x.IsConfirmed)
            .OrderBy(x=>x.StartSlotIndex)
            .Select(x=>new{ x.AppointmentId, x.StartSlotIndex, x.SlotCount})
            .ToListAsync(ct);

        var timespans = slots.Select(x =>
            new ScheduleDto(
                x.AppointmentId,
                options.Value.GetSlotTime(x.StartSlotIndex),
                options.Value.GetSlotTime(x.StartSlotIndex + x.SlotCount)
            )).ToList();

        return new GetScheduleResponse(timespans);
    }
}

public record ScheduleDto(Guid AppointmentId, TimeSpan BeginTime, TimeSpan EndTime);