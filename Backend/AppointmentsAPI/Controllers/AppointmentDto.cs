using System.Linq.Expressions;
using AppointmentsAPI.Models;

namespace AppointmentsAPI.Controllers;

public class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientAccountId { get; init; }
    public Guid DoctorAccountId { get; init; }
    public long? ReservationId { get; init; }
    public DateOnly Date { get; init; }
    public int StartSlotIndex { get; init; }
    public long ServiceId { get; init; }
    public string State { get; init; }
}

public static class AppointmentDtoHelper
{

    public static Expression<Func<Appointment, AppointmentDto>> ProjectToDto => 
        a => new AppointmentDto
        {
            Id = a.Id,
            PatientAccountId = a.PatientAccountId,
            DoctorAccountId = a.DoctorAccountId,
            Date = a.Date,
            StartSlotIndex = a.StartSlotIndex,
            ServiceId = a.ServiceId,
            State = a.State.ToString(),
            ReservationId = a.ReservationId
        };
    
}