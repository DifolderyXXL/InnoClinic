namespace Contracts.AppointmentContracts;



// Consumer
public record AppointmentSubmitted(
    Guid AppointmentId,
    Guid PatientAccountId,
    Guid DoctorAccountId,
    DateOnly Date,
    int StartSlotIndex,
    long ServiceId);
public record TimeWindowReserved(Guid AppointmentId, long ReservationId, TimeSpan BeginTime, TimeSpan EndTime);
public record ReservationFailed(Guid AppointmentId);
public record AppointmentApproved(Guid AppointmentId);
public record AppointmentDeclined(Guid AppointmentId, string? Reason);
public record ReservationExpired(Guid AppointmentId, long ReservationId);
public record ReservationConfirmed(Guid AppointmentId, long ReservationId);


// Appointment Producer
public class ProcessReservation
{
    public ProcessReservation() { }

    public ProcessReservation(Guid appointmentId, Guid doctorId, DateOnly date, int startSlotIndex, long serviceId)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        Date = date;
        StartSlotIndex = startSlotIndex;
        ServiceId = serviceId;
    }

    public Guid AppointmentId { get; init; }
    public Guid DoctorId { get; init;  }
    public DateOnly Date { get; init; }
    public int StartSlotIndex { get; init; }
    public long ServiceId { get; init; }
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