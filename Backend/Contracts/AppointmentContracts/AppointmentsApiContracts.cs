namespace Contracts.AppointmentContracts;



// Consumer
public record AppointmentSubmitted(Guid AppointmentId, Guid PatientAccountId, long DoctorId, DateOnly Date, int StartSlotIndex, int SlotCount);
public record TimeWindowReserved(Guid AppointmentId, int ReservationId);
public record AppointmentApproved(Guid AppointmentId);
public record AppointmentDeclined(Guid AppointmentId, string? Reason);
public record ReservationExpired(Guid AppointmentId, int ReservationId);
public record ReservationConfirmed(Guid AppointmentId, int ReservationId);


// Appointment Producer
public record ProcessReservation(Guid AppointmentId, DateOnly Date, int StartSlotIndex, int SlotCount);
public record ProcessReservationConfirmation(Guid AppointmentId, long ReservationId);
public record CancelReservation(long ReservationId);