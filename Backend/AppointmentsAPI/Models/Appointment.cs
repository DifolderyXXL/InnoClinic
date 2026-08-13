namespace AppointmentsAPI.Models;

public class Appointment
{
    public Guid Id { get; set; }
    
    public Guid PatientAccountId { get; set; }
    public Guid DoctorAccountId { get; set; }


    public long? ReservationId { get; set; } 
    public TimeSpan? BeginTime { get; set; }
    public TimeSpan? EndTime { get; set; }

    public string DoctorFullName { get; set; }
    public string PatientFullName { get; set; }
    public string PatientEmail { get; set; }
    public string ServiceName { get; set; }
    public string CategoryName { get; set; }
    public string SpecializationName { get; set; }
    
    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotAmount { get; set; }
    public long ServiceId { get; set; }
    public string OfficeId { get; set; }
    public long SpecializationId { get; set; }
    
    public AppointmentState State { get; set; }
}