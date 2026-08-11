namespace Contracts.Notifications;

public record UserRegisteredIntegrationEvent(
    Guid UserId, 
    string Email, 
    string CreateAccountLink
);

public class UserAppointmentConfirmedIntegrationEvent
{
    public Guid AppointmentId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan BeginTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Email { get; set; }
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