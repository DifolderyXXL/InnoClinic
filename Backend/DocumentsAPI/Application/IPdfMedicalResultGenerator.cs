using MicroserviceApiKernel.Results;

namespace DocumentsAPI.Application;

public interface IPdfMedicalResultGenerator
{
    public byte[] Generate(MedicalResultPdfData data);
}

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

public record UserFullName(string FirstName, string LastName, string? MiddleName)
{
    public override string ToString()
    {
        var name = $"{FirstName} {LastName}";
        if (MiddleName != null) name += $" {MiddleName}";
        
        return name;
    }
}