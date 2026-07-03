using System.Diagnostics;
using AppointmentsAPI.Data;
using AppointmentsAPI.ModelBinders;
using AppointmentsAPI.Models;
using Contracts.AppointmentContracts;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public record BookAppointmentCommand(long DoctorId, DateOnly Date, int StartSlotIndex, int SlotCount);

public record DeclineCommand(string Reason);

[Route("api/[controller]")]
[ApiController]
public class AppointmentController : ControllerBase
{
    [HttpPost]
    [Route("/book")]
    [Authorize(Policy = RolePolicy.Client)]
    public async Task<IActionResult> BookAppointment(
        [ModelBinder<UserClaimsInfoModelBinder>] UserClaimParserResult? user,
        [FromBody] BookAppointmentCommand command,
        IAppointmentService appointmentService,
        IPublishEndpoint publishEndpoint,
        CancellationToken ct)
    {
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
            State = AppointmentState.Created,
            DoctorId = command.DoctorId,
            Date = command.Date,
            StartSlotIndex = command.StartSlotIndex,
            SlotCount = command.SlotCount
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
                command.SlotCount), ct);

        return Accepted(result.Value);
    }
    
    [HttpPost]
    [Route("/approve-book/{id:guid}")]
    [Authorize(Policy = RolePolicy.Receptionist)]
    public async Task<IActionResult> ApproveAppointment(
        [FromRoute] Guid id,
        IPublishEndpoint publishEndpoint,
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
        IPublishEndpoint publishEndpoint,
        CancellationToken ct)
    {
        await publishEndpoint.Publish(new AppointmentDeclined(id, command.Reason), ct);

        return Accepted();
    }
}

public interface IAppointmentService
{
    public Task<Result<Guid>> AddAppointment(Appointment appointment, CancellationToken ct);
    public Task<Result> UpdateState(Guid appointmentId, AppointmentState state, CancellationToken ct);
}

public class AppointmentService(AppointmentDbContext context) : IAppointmentService
{
    public async Task<Result<Guid>> AddAppointment(Appointment appointment, CancellationToken ct)
    {
        await context.Appointments.AddAsync(appointment, ct);
        await context.SaveChangesAsync(ct);

        return appointment.Id;
    }

    public async Task<Result> UpdateState(Guid appointmentId, AppointmentState state, CancellationToken ct)
    {
        var appointment = await context.Appointments.FindAsync([appointmentId], ct);

        if (appointment == null)
        {
            return AppointmentErrors.AppointmentNotFound();
        }
        
        appointment.State = state;
        
        await context.SaveChangesAsync(ct);
        return Result.Success();
    }
}

public static class AppointmentErrors
{
    public static Error AppointmentNotFound() => Error.Create(ErrorType.NotFound);
}