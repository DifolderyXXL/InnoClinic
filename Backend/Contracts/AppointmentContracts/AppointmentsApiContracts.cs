namespace Contracts.AppointmentContracts;



// Consumer
public record AppointmentSubmitted(Guid AppointmentId, Guid PatientAccountId, long DoctorId, DateOnly Date, int StartSlotIndex, int SlotCount);
public record TimeWindowReserved(Guid AppointmentId, long ReservationId);
public record ReservationFailed(Guid AppointmentId);
public record AppointmentApproved(Guid AppointmentId);
public record AppointmentDeclined(Guid AppointmentId, string? Reason);
public record ReservationExpired(long ReservationId);
public record ReservationConfirmed(Guid AppointmentId, long ReservationId);


// Appointment Producer
public class ProcessReservation
{
    public ProcessReservation() { }

    public ProcessReservation(Guid appointmentId, DateOnly date, int startSlotIndex, int slotCount)
    {
        AppointmentId = appointmentId;
        Date = date;
        StartSlotIndex = startSlotIndex;
        SlotCount = slotCount;
    }

    public Guid AppointmentId { get; init; }
    public DateOnly Date { get; init; }
    public int StartSlotIndex { get; init; }
    public int SlotCount { get; init; }
}

public class ProcessReservationConfirmation
{
    public ProcessReservationConfirmation() { }

    public ProcessReservationConfirmation(Guid appointmentId, long reservationId)
    {
        AppointmentId = appointmentId;
        ReservationId = reservationId;
    }

    public Guid AppointmentId { get; init; }
    public long ReservationId { get; init; }
}

public class CancelReservation
{
    public CancelReservation() { }

    public CancelReservation(long reservationId)
    {
        ReservationId = reservationId;
    }

    public long ReservationId { get; init; }
}