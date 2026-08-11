using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using AppointmentsAPI.Services;
using Asp.Versioning;
using Contracts.AppointmentContracts;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions.Endpoints;
using MicroserviceApiKernel.Extensions.Queryable;
using MicroserviceApiKernel.Results;
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
    [HasPermission(Permissions.Appointments.ManageOwn)]
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

        var result = await ExecuteBookingAsync(
            patientId,
            command,
            false,
            profilesApiClient,
            servicesApiClient,
            ct);

        if (result.IsError)
            return BadRequest(result.Error);

        return Accepted(result.Value);
    }
    
    [HttpPost]
    [Route("users/{userId:guid}")]
    [HasPermission(Permissions.Appointments.Manage)]
    [ProducesResponseType(typeof(PagedResponse<Guid>), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> BookAppointment(
        [FromRoute] Guid userId,
        [FromBody] BookAppointmentCommand command,
        [FromServices] IProfilesApiClient profilesApiClient,
        [FromServices] IServicesApiClient servicesApiClient,
        CancellationToken ct)
    {
        var result = await ExecuteBookingAsync(
            userId,
            command,
            true,
            profilesApiClient,
            servicesApiClient,
            ct);

        if (result.IsError)
            return BadRequest(result.Error);

        return Accepted(result.Value);
    }
    
    private async Task<Result<Guid>> ExecuteBookingAsync(
        Guid patientId,
        BookAppointmentCommand command,
        bool isCreatedByAdmin,
        IProfilesApiClient profilesApiClient,
        IServicesApiClient servicesApiClient,
        CancellationToken ct)
    {
        var profilesTask = profilesApiClient.ValidateAppointmentContextAsync(
            new(command.DoctorAccountId, patientId, command.OfficeId), ct);
        
        var serviceTask = servicesApiClient.GetService(command.ServiceId, ct);

        await Task.WhenAll(profilesTask, serviceTask);

        var profilesResult = await profilesTask;
        var serviceResult = await serviceTask;

        if (profilesResult.IsError) return profilesResult.Error;
        if (serviceResult.IsError) return serviceResult.Error;

        var profiles = profilesResult.Value!;
        var service = serviceResult.Value!;

        if (command.SpecializationId != service.SpecializationId)
        {
            return Error.Validation("Booking.SpecializationMismatch", 
                "Selected specialization does not match the requested service.");
        }

        if (profiles.DoctorSpecializationId != service.SpecializationId)
        {
            return Error.Validation("Booking.DoctorSpecializationMismatch", 
                "Doctor's specialization does not match the requested service.");
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
            PatientEmail = profiles.Email,
            ServiceName = service.ServiceName,
            CategoryName = service.CategoryName,
            SpecializationName = service.SpecializationName,
        };

        var result = await appointmentService.AddAppointment(appointment, ct);
        if (result.IsError) return result.Error;

        await publishEndpoint.Publish(
            new AppointmentSubmitted(
                result.Value,
                patientId,
                command.DoctorAccountId,
                profiles.Email,
                command.Date,
                command.StartSlotIndex,
                command.ServiceId,
                isCreatedByAdmin), ct);

        await context.SaveChangesAsync(ct);

        return result.Value;
    }

    [HttpPost("{id:guid}/approve")]
    [HasPermission(Permissions.Appointments.Manage)]
    public async Task<IActionResult> ApproveAppointment(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await publishEndpoint.Publish(new AppointmentApproved(id), ct);
        await context.SaveChangesAsync(ct);

        return Accepted();
    }

    [HttpPost("{id:guid}/decline")]
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

    [HttpGet("clinic")]
    [HasPermission(Permissions.Appointments.Read)]
    [ProducesResponseType(typeof(PagedResponse<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClinicAppointments(
        [FromQuery] ClinicAppointmentsFilterParameters filter,
        [FromQuery] PaginationParameters pagination,
        CancellationToken ct = default)
    {
        var items = await context.QueryClinicAppointmentsAsync(filter, pagination, ct);
        return Ok(items);
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
            doctorId: doctorId);

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

    [HttpGet("{id:guid}")]
    [Authorize(RolePolicy.IdentityServer)]
    [ProducesResponseType(typeof(AppointmentInformationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAppointmentInfo(
        [FromRoute] Guid id,
        CancellationToken ct = default)
    {
        var query = context.Appointments.AsNoTracking();

        var item = await query
            .Where(x => x.Id == id)
            .Select(a => new AppointmentInformationDto
            {
                PatientEmail = a.PatientEmail,
                Date = a.Date,
                BeginTime = a.BeginTime,
                EndTime = a.EndTime,
            })
            .FirstOrDefaultAsync(ct);
        
        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    public class AppointmentInformationDto
    {
        public string PatientEmail { get; init; }
        public DateOnly Date { get; init; }

        public TimeSpan? BeginTime { get; init; }
        public TimeSpan? EndTime { get; init; }
    }

}

public record ClinicAppointmentsFilterParameters(
    DateOnly? Date,
    string? DoctorFullName,
    string? ServiceName,
    string? OfficeId,
    AppointmentState? Status
);

public static class AppointmentExtensions
{
    /// <summary>
    /// Сортировка согласно AC-12, AC-13, AC-14:
    /// 1. По времени приема (StartSlotIndex / BeginTime)
    /// 2. По Фамилии доктора (Ascending)
    /// 3. По Имени доктора (Ascending)
    /// 4. По Названию услуги (Ascending)
    /// </summary>
    public static IQueryable<Appointment> OrderClinicAppointments(this IQueryable<Appointment> query)
    {
        return query
            // 1. Сортировка по времени приема
            .OrderByDescending(a => a.Date)
            .ThenBy(a => a.StartSlotIndex) 
            .ThenBy(a => a.DoctorFullName)
            .ThenBy(a => a.ServiceName);
    }

    public static async Task<PagedResponse<AppointmentDto>> QueryClinicAppointmentsAsync(
        this AppointmentDbContext context,
        ClinicAppointmentsFilterParameters filter,
        PaginationParameters pagination,
        CancellationToken ct)
    {
        var query = context.Appointments.AsNoTracking();

        // AC-7: Фильтрация по дате приёма
        if (filter.Date.HasValue)
        {
            query = query.Where(x => x.Date == filter.Date.Value);
        }

        // AC-8: Фильтрация по ФИО доктора (регистронезависимый поиск)
        if (!string.IsNullOrWhiteSpace(filter.DoctorFullName))
        {
            query = query.Where(x => EF.Functions.Like(x.DoctorFullName, $"%{filter.DoctorFullName}%"));
        }

        // AC-9: Фильтрация по названию услуги
        if (!string.IsNullOrWhiteSpace(filter.ServiceName))
        {
            query = query.Where(x => EF.Functions.Like(x.ServiceName, $"%{filter.ServiceName}%"));
        }

        // AC-10: Фильтрация по статусу 
        if (filter.Status.HasValue)
        {
            query = query.Where(x=> x.State == filter.Status.Value);
        }

        // AC-11: Фильтрация по офису
        if (!string.IsNullOrWhiteSpace(filter.OfficeId))
        {
            query = query.Where(x => x.OfficeId == filter.OfficeId);
        }

        // Применяем кастомную сортировку (AC-12, AC-13, AC-14) и пагинацию
        return await query
            .OrderClinicAppointments()
            .ToPagedResponseAsync(
                pagination,
                AppointmentDtoHelper.ProjectToDto,
                ct);
    }
    
    
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
