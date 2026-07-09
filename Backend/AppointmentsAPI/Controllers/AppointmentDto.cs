using System.Linq.Expressions;
using AppointmentsAPI.Models;

namespace AppointmentsAPI.Controllers;

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

public static class AppointmentDtoHelper
{
    public static IQueryable<AppointmentDto> MapToDto(this IQueryable<Appointment> query)
    {
        return query.Select(a => new AppointmentDto
        {
            Id = a.Id,
            PatientAccountId = a.PatientAccountId,
            DoctorId = a.DoctorId,
            Date = a.Date,
            StartSlotIndex = a.StartSlotIndex,
            ServiceId = a.ServiceId,
            State = a.State.ToString(),
            ReservationId = a.ReservationId
        });
    }

    public static Expression<Func<Appointment, AppointmentDto>> ProjectToDto => 
        a => new AppointmentDto
        {
            Id = a.Id,
            PatientAccountId = a.PatientAccountId,
            DoctorId = a.DoctorId,
            Date = a.Date,
            StartSlotIndex = a.StartSlotIndex,
            ServiceId = a.ServiceId,
            State = a.State.ToString(),
            ReservationId = a.ReservationId
        };
    
}