using System.Diagnostics;
using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using Contracts.AppointmentContracts;
using MassTransit;
using MicroserviceApiKernel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public record BookAppointmentCommand(long DoctorId, long OfficeId, DateOnly Date, int StartSlotIndex, long ServiceId, long SpecializationId);

public record DeclineCommand(string Reason);

[Route("api/[controller]")]
[ApiController]
public class AppointmentController(
    IPublishEndpoint publishEndpoint, 
    IAppointmentService appointmentService,
    AppointmentDbContext context) : ControllerBase
{
    private async ValueTask<UserClaimParserResult?> GetUserClaim() => await UserClaimParser.Parse(HttpContext);
    
    [HttpPost]
    [Route("/book")]
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
            DoctorId = command.DoctorId,
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
                command.DoctorId,
                command.Date,
                command.StartSlotIndex,
                command.ServiceId), ct);
        await context.SaveChangesAsync(ct);

        return Accepted(result.Value);
    }
    
    [HttpPost]
    [Route("/approve-book/{id:guid}")]
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
    [Route("/decline-book/{id:guid}")]
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
    [Route("/appointments")]
    [Authorize(Policy = RolePolicy.Receptionist)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] AppointmentState? state,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
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
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentDto
            {
                Id = a.Id,
                PatientAccountId = a.PatientAccountId,
                DoctorId = a.DoctorId,
                Date = a.Date,
                StartSlotIndex = a.StartSlotIndex,
                ServiceId = a.ServiceId,
                State = a.State.ToString(),
                ReservationId = a.ReservationId
            })
            .ToListAsync(ct);
        
        return Ok(new{ Items = items, Total = total, Page = page, PageSize = pageSize });
    }
}

public class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientAccountId { get; init; }
    public long DoctorId { get; init; }
    public long? ReservationId { get; init; }
    public DateOnly Date { get; init; }
    public int StartSlotIndex { get; init; }
    public long ServiceId { get; init; }
    public string State { get; init; }
}