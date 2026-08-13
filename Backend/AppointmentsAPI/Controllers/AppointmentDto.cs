using System.Linq.Expressions;
using AppointmentsAPI.Models;

namespace AppointmentsAPI.Controllers;

public class AppointmentDto
{
    public Guid Id { get; init; }
    public Guid PatientAccountId { get; init; }
    public Guid DoctorAccountId { get; init; }
    public string OfficeId { get; init; } = null!;
    public long ServiceId { get; init; }
    public long SpecializationId { get; init; }
    
    public string DoctorFullName { get; init; } = null!;
    public string PatientFullName { get; init; } = null!;
    public string ServiceName { get; init; } = null!;
    
    public long? ReservationId { get; init; }
    public DateOnly Date { get; init; }
    public int StartSlotIndex { get; init; }
    public int SlotAmount { get; set; }
    
    public TimeSpan? BeginTime { get; init; }
    public TimeSpan? EndTime { get; init; }
    
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
            OfficeId = a.OfficeId,
            ServiceId = a.ServiceId,
            SpecializationId = a.SpecializationId,
            
            DoctorFullName = a.DoctorFullName,
            PatientFullName = a.PatientFullName,
            ServiceName = a.ServiceName,
            
            ReservationId = a.ReservationId,
            Date = a.Date,
            StartSlotIndex = a.StartSlotIndex,
            SlotAmount = a.SlotAmount,
            BeginTime = a.BeginTime,
            EndTime = a.EndTime,
            
            State = a.State.ToString()
        };
    
}