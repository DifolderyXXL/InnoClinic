namespace DocumentsAPI.Application;

public record struct AppointmentKey(Guid PatientId, Guid AppointmentId);