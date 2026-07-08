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