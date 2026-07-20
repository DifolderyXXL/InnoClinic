namespace DocumentsAPI.Application;

public record MedicalResultPdfData(
    Guid AppointmentId,
    UserFullName Doctor,
    string Specialization,
    string ServiceName,
    UserFullName Patient,
    DateOnly PatientDateOfBirth,
    string Complaints,
    string Conclusion,
    string Diagnosis,
    string Recommendations,
    DateTimeOffset Date
);