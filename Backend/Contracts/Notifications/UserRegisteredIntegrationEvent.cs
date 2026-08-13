namespace Contracts.Notifications;

public record UserRegisteredIntegrationEvent(
    Guid UserId, 
    string Email, 
    string CreateAccountLink
);

public class UserAppointmentConfirmedIntegrationEvent
{
    public required string PatientEmail { get; init; }
    public required string PatientName { get; init; }
    public required string DoctorName { get; init; }
    public required string ServiceName { get; init; }
    public required string SpecializationName { get; init; }
    public required string CategoryName { get; init; }
    public required DateOnly Date { get; init; }
    public required TimeSpan BeginTime { get; init; }
    public required TimeSpan EndTime { get; init; }
}

public record MedicalResultUpdatedIntegrationEvent(
    Guid AppointmentId,
    DateTimeOffset UpdateStamp,
    string DoctorName,
    string Specialization,
    string ServiceName,
    string Complaints,
    string Conclusion,
    string Diagnosis,
    string Recommendations
);