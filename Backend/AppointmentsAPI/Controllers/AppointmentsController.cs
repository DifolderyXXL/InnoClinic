using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using AppointmentsAPI.Services;
using Asp.Versioning;
using Contracts.AppointmentContracts;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions.Endpoints;
using MicroserviceApiKernel.Extensions.Queryable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

public record BookAppointmentCommand(Guid DoctorAccountId, string OfficeId, DateOnly Date, int StartSlotIndex, long ServiceId, long SpecializationId);
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
    [HasPermission(Permissions.Appointments.Manage)]
    [ProducesResponseType(typeof(PagedResponse<Guid>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BookAppointment(
        [FromBody] BookAppointmentCommand command,
        [FromServices] IProfilesApiClient profilesApiClient,
        [FromServices] IServicesApiClient servicesApiClient,
        CancellationToken ct)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var patientId))
        {
            return Unauthorized();
        }

        var profilesResultTask = profilesApiClient.ValidateAppointmentContextAsync(new(command.DoctorAccountId, patientId, command.OfficeId), ct);
        var serviceResultTask = servicesApiClient.GetService(command.ServiceId, ct);

        await Task.WhenAll(profilesResultTask, serviceResultTask);

        var profilesResult = await profilesResultTask;
        var serviceResult = await serviceResultTask;

        if (profilesResult.IsError)
            return BadRequest(profilesResult.Error);
        if (serviceResult.IsError)
            return BadRequest(serviceResult.Error);

        var profiles = profilesResult.Value!;
        var service = serviceResult.Value!;

        if (command.SpecializationId != service.SpecializationId)
        {
            return BadRequest("Selected specialization does not match the requested service.");
        }

        if (profiles.DoctorSpecializationId != service.SpecializationId)
        {
            return BadRequest("Doctor's specialization does not match the requested service.");
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
            SpecializationId = command.SpecializationId,

            DoctorFullName = profiles.DoctorFullName,
            PatientFullName = profiles.PatientFullName,
            ServiceName = service.ServiceName,
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
    [HasPermission(Permissions.Appointments.Manage)]
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
    [HasPermission(Permissions.Appointments.Manage)]
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
    [HasPermission(Permissions.Appointments.Read)]
    public async Task<IActionResult> GetAppointments(
        [FromQuery] AppointmentState? state,
        [FromQuery] Guid? patientId,
        [FromQuery] PaginationParameters pagination,
        CancellationToken ct = default)
    {
        var items = await context.QueryAppointmentsAsync(pagination, ct, state, 
            patientId: patientId);
        
        return Ok(items);
    }

    [HttpGet("me/client")]
    [HasPermission(Permissions.Appointments.ReadOwn)]
    [ProducesResponseType(typeof(PagedResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClientAppointments(
        [FromQuery] AppointmentState? state,
        [FromQuery] PaginationParameters pagination,
        CancellationToken ct = default)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var clientId))
        {
            return Unauthorized();
        }
        
        var items = await context.QueryAppointmentsAsync(pagination, ct, state, 
            patientId: clientId);

        return Ok(items);
    }


    [HttpGet("me/doctor")]
    [HasPermission(Permissions.Appointments.ReadOwn)]
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

        var items = await context.QueryAppointmentsAsync(pagination, ct, state, 
            doctorId:doctorId);

        return Ok(items);
    }

    [HttpGet("{id:guid}/me/client")]
    [HasPermission(Permissions.Appointments.ReadOwn)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClientAppointments(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var clientId))
        {
            return Unauthorized();
        }
        
        var query = context.Appointments.AsNoTracking();
        
        var item = await query
            .Where(x => x.Id == id && x.PatientAccountId == clientId)
            .Select(AppointmentDtoHelper.ProjectToDto)
            .FirstOrDefaultAsync(ct);
        
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpGet("{id:guid}/me/doctor")]
    [HasPermission(Permissions.Appointments.ReadOwn)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDoctorAppointment(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var user = await GetUserClaim();
        if (user == null || !Guid.TryParse(user.Id, out var clientId))
        {
            return Unauthorized();
        }
        
        var query = context.Appointments.AsNoTracking();
        
        var item = await query
            .Where(x => x.Id == id && x.DoctorAccountId == clientId)
            .Select(AppointmentDtoHelper.ProjectToDto)
            .FirstOrDefaultAsync(ct);
        
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}

public static class AppointmentExtensions
{
    public static IQueryable<Appointment> OrderAppointments(this IQueryable<Appointment> query)
    {
        return query.OrderByDescending(x => x.Date)
            .ThenBy(x => x.BeginTime);
    }

    public static async Task<PagedResponse<AppointmentDto>> QueryAppointmentsAsync(
        this AppointmentDbContext context,
        PaginationParameters pagination,
        CancellationToken ct,
        AppointmentState? state = null, Guid? doctorId= null, Guid? patientId= null)
    {
        
        var query = context.Appointments.AsNoTracking();

        if (state != null)
        {
            query = query.Where(x => x.State == state);
        }

        if (doctorId != null)
        {
            query = query.Where(x => x.DoctorAccountId == doctorId);
        }
        
        if (patientId != null)
        {
            query = query.Where(x => x.PatientAccountId == patientId);
        }

        var items = await query
            .OrderAppointments()
            .ToPagedResponseAsync(
                pagination,
                AppointmentDtoHelper.ProjectToDto,
                ct);
        return items;
    }
}
