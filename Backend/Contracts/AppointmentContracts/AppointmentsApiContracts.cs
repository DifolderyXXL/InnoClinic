namespace Contracts.AppointmentContracts;

// Consumer
public record AppointmentSubmitted(
    Guid AppointmentId,
    Guid PatientAccountId,
    Guid DoctorAccountId,
    string PatientEmail,
    DateOnly Date,
    int StartSlotIndex,
    long ServiceId,
    bool IsCreatedByAdmin
    );

public record AppointmentRescheduleRequested(Guid AppointmentId, DateOnly NewDate, int NewStartSlotIndex);
public record AppointmentRescheduled(
    Guid AppointmentId, 
    long NewReservationId,  
    DateOnly NewDate, 
    int NewStartSlotIndex,
    TimeSpan NewBeginTime, 
    TimeSpan NewEndTime
);
public record AppointmentRescheduleFailed(
    Guid AppointmentId, 
    string Reason
);

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

    public ProcessReservation(Guid appointmentId, Guid doctorId, DateOnly date, int startSlotIndex, long serviceId, Guid patientId)
    {
        AppointmentId = appointmentId;
        DoctorId = doctorId;
        Date = date;
        StartSlotIndex = startSlotIndex;
        ServiceId = serviceId;
        PatientId = patientId;
    }

    public Guid AppointmentId { get; init; }
    public Guid DoctorId { get; init; }
    public Guid PatientId { get; init; }
    public DateOnly Date { get; init; }
    public int StartSlotIndex { get; init; }
    public long ServiceId { get; init; }
}


public class ProcessRescheduleReservation
{
    public ProcessRescheduleReservation() { }

    public ProcessRescheduleReservation(
        Guid appointmentId, 
        long currentReservationId, 
        Guid doctorId, 
        Guid patientId, 
        DateOnly newDate, 
        int newStartSlotIndex, 
        long serviceId)
    {
        AppointmentId = appointmentId;
        CurrentReservationId = currentReservationId;
        DoctorId = doctorId;
        PatientId = patientId;
        NewDate = newDate;
        NewStartSlotIndex = newStartSlotIndex;
        ServiceId = serviceId;
    }

    public Guid AppointmentId { get; init; }
    public long CurrentReservationId { get; init; }
    public Guid DoctorId { get; init; }
    public Guid PatientId { get; init; }
    public DateOnly NewDate { get; init; }
    public int NewStartSlotIndex { get; init; }
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

public record AppointmentConfirmedIntegrationEvent
{
    public Guid AppointmentId { get; init; }
}