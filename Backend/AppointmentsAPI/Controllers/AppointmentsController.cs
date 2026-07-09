using System.Diagnostics;
using System.Linq.Expressions;
using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using Asp.Versioning;
using Contracts.AppointmentContracts;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions.Queryable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public record BookAppointmentCommand(Guid DoctorAccountId, long OfficeId, DateOnly Date, int StartSlotIndex, long ServiceId, long SpecializationId);
public record DeclineCommand(string Reason);


[Route("api/v{v:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1)]
public class AppointmentsController(
    IPublishEndpoint publishEndpoint, 
    IAppointmentService appointmentService,
    AppointmentDbContext context) : ControllerBase
{
    private async ValueTask<UserClaimParserResult?> GetUserClaim() => await UserClaimParser.Parse(HttpContext);
    
    [HttpPost]
    [Route("book")]
    [Authorize(Policy = RolePolicy.Client)]
    public async Task<IActionResult> BookAppointment(
        [FromBody] BookAppointmentCommand command,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var patientId))
        {
            return Unauthorized();
        }

        var appointment = new Appointment
        {
            PatientAccountId = patientId,
            State = AppointmentState.Created,
            DoctorAccountId = command.DoctorAccountId,
            Date = command.Date,
            StartSlotIndex = command.StartSlotIndex,
            ServiceId = command.ServiceId,
            OfficeId = command.OfficeId,
            SpecializationId = command.SpecializationId
        };
        var result = await appointmentService.AddAppointment(appointment, ct);
        if (result.IsError) return BadRequest();
        
        await publishEndpoint.Publish(
            new AppointmentSubmitted(
                result.Value,
                patientId,
                command.DoctorAccountId,
                command.Date,
                command.StartSlotIndex,
                command.ServiceId), ct);
        await context.SaveChangesAsync(ct);

        return Accepted(result.Value);
    }
    
    [HttpPost]
    [Route("approve-book/{id:guid}")]
    [Authorize(Policy = RolePolicy.Receptionist)]
    public async Task<IActionResult> ApproveAppointment(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await publishEndpoint.Publish(new AppointmentApproved(id), ct);
        await context.SaveChangesAsync(ct);

        return Accepted();
    }

    [HttpPost]
    [Route("decline-book/{id:guid}")]
    [Authorize(Policy = RolePolicy.Receptionist)]
    public async Task<IActionResult> DeclineAppointment(
        [FromRoute] Guid id,
        [FromBody] DeclineCommand command,
        CancellationToken ct)
    {
        await publishEndpoint.Publish(new AppointmentDeclined(id, command.Reason), ct);
        await context.SaveChangesAsync(ct);

        return Accepted();
    }
    
    [HttpGet]
    [Authorize(Policy = RolePolicy.Receptionist)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] AppointmentState? state,
        [FromQuery] PaginationParameters pagination,
        CancellationToken ct = default)
    {
        var query = context.Appointments.AsNoTracking();

        if (state != null)
        {
            query = query.Where(x => x.State == state);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Id)
            .Pagination(pagination)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientAccountId = a.PatientAccountId,
                DoctorAccountId = a.DoctorAccountId,
                Date = a.Date,
                StartSlotIndex = a.StartSlotIndex,
                ServiceId = a.ServiceId,
                State = a.State.ToString(),
                ReservationId = a.ReservationId
            })
            .ToListAsync(ct);
        
        return Ok(new{ Items = items, Total = total, Page = pagination.Page, PageSize = pagination.PageSize });
    }
    
    [HttpGet("me")]
    [Authorize(Policy = RolePolicy.Doctor)]
    [ProducesResponseType(typeof(PagedResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorAppointments(
        [FromQuery] AppointmentState? state,
        [FromQuery] PaginationParameters pagination,
        CancellationToken ct = default)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var doctorId))
        {
            return Unauthorized();
        }
        
        var query = context.Appointments.AsNoTracking();

        if (state != null)
        {
            query = query.Where(x => x.State == state);
        }

        var items = await query
            .Where(x => x.DoctorAccountId == doctorId)
            .OrderBy(x => x.Id)
            .ToPagedResponseAsync(
                pagination, 
                AppointmentDtoHelper.ProjectToDto,
                ct);
        
        return Ok(items);
    }
}


