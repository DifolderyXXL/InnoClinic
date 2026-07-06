using System.Diagnostics;
using AppointmentsAPI.Data;
using AppointmentsAPI.ModelBinders;
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

public record BookAppointmentCommand(long DoctorId, DateOnly Date, int StartSlotIndex, long ServiceId);

public record DeclineCommand(string Reason);

[Route("api/[controller]")]
[ApiController]
public class AppointmentController(
    IPublishEndpoint publishEndpoint, 
    IAppointmentService appointmentService,
    IServiceProvider provider) : ControllerBase
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
        if (user == null)
        {
            return Problem(statusCode: StatusCodes.Status500InternalServerError);
        }

        if (!Guid.TryParse(user.Id, out var patientId))
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
            ServiceId = command.ServiceId
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

        return Accepted();
    }
    
    [HttpGet]
    [Route("/appointments")]
    [Authorize(Policy = RolePolicy.Receptionist)]
    public async Task<IActionResult> GetAppointments(
        AppointmentState? state,
        CancellationToken ct)
    {
        var context = provider.GetRequiredService<AppointmentDbContext>();

        var query = context.Appointments.AsNoTracking();

        if (state != null)
        {
            query = query.Where(x => x.State == state);
        }

        var result = await query.Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientAccountId = a.PatientAccountId,
            DoctorId = a.DoctorId,
            Date = a.Date,
            StartSlotIndex = a.StartSlotIndex,
            ServiceId = a.ServiceId,
            State = a.State.ToString(),
            ReservationId = a.ReservationIdUnsafe
        }).ToListAsync(ct);
        return Ok(result);
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