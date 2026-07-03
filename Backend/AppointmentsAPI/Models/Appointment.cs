namespace AppointmentsAPI.Models;

public class Appointment
{
    public Guid Id { get; set; }
    
    public Guid PatientAccountId { get; set; }
    public long DoctorId { get; set; }

    private long? _reservationId;

    public long ReservationId
    {
        get => _reservationId ?? throw new InvalidOperationException("Is not reserved");
        set => _reservationId = value;
    }

    public DateOnly Date { get; set; }
    public int StartSlotIndex { get; set; }
    public int SlotCount { get; set; }
    
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