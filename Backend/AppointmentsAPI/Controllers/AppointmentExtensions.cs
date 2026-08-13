using AppointmentsAPI.Data;
using AppointmentsAPI.Models;
using MicroserviceApiKernel.Extensions.Queryable;
using Microsoft.EntityFrameworkCore;
using AppointmentState = AppointmentsAPI.Models.AppointmentState;

namespace AppointmentsAPI.Controllers;

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