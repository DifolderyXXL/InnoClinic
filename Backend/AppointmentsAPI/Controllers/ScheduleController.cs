using AppointmentsAPI.Data;
using MicroserviceApiKernel;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Extensions.Endpoints;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
using MicroserviceApiKernel.SharedControllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public class ScheduleController(IQueryHandler<GetScheduleQuery, List<AppointmentDto>> handler) : BaseApiController
{
    [HttpGet("today/me")]
    [HasPermission(Permissions.Schedules.ReadOwn)]
    public async Task<IActionResult> GetTodayMySchedule(
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null) return Unauthorized();
        
        var id = Guid.Parse(user.Id);
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await handler.Handle(new GetScheduleQuery(date, id), ct);
        
        return Ok(result.Value);
    }

    [HttpGet("me")]
    [HasPermission(Permissions.Schedules.ReadOwn)]
    public async Task<IActionResult> GetMySchedule(
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null) return Unauthorized();
        
        var id = Guid.Parse(user.Id);

        var result = await handler.Handle(new GetScheduleQuery(date, id), ct);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Schedules.Read)]
    public async Task<IActionResult> GetScheduleById(
        [FromRoute] Guid id,
        [FromQuery] DateOnly date,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetScheduleQuery(date, id), ct);

        return Ok(result.Value);
    }

    [HttpGet("today/{id:guid}")]
    [HasPermission(Permissions.Schedules.Read)]
    public async Task<IActionResult> GetTodayScheduleById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
            
        var result = await handler.Handle(new GetScheduleQuery(date, id), ct);

        return Ok(result.Value);
    }
}

public record GetScheduleQuery(DateOnly Date, Guid DoctorId) : IQuery<List<AppointmentDto>>;



public class GetScheduleQueryHandler(AppointmentDbContext context) : IQueryHandler<GetScheduleQuery, List<AppointmentDto>>
{
    public async Task<Result<List<AppointmentDto>>> Handle(GetScheduleQuery queryC, CancellationToken ct)
    {
        var query = context.Appointments.AsNoTracking();
        
        var items = await query
            .Where(a=>a.DoctorAccountId == queryC.DoctorId)
            .Where(a=>a.Date == queryC.Date)
            .Where(a=>a.State == AppointmentState.Confirmed)
            .Select(AppointmentDtoHelper.ProjectToDto)
            .ToListAsync(ct);

        return items;
    }
}
