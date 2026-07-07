namespace AppointmentsAPI.Models;

public class Appointment
{
    public Guid Id { get; set; }
    
    public Guid PatientAccountId { get; set; }
    public long DoctorId { get; set; }

    public long? ReservationId { get; set; } 

    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public long ServiceId { get; set; }
    public long OfficeId { get; set; }
    public long SpecializationId { get; set; }
    
    public AppointmentState State { get; set; }
}

public enum AppointmentState
{
    Created,
    PendingReservation,
    PendingApproval,
    Approved,
    Failed,
    Confirmed
}