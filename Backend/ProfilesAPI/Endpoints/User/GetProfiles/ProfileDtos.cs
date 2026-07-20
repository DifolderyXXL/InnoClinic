using ProfilesAPI.Models;

namespace ProfilesAPI.Endpoints.User.GetProfiles;

public record BaseAccountDto(
    Guid Id,
    string Email,
    string? PhoneNumber,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid? PhotoId
);

public record PatientDto(
    long Id,
    DateOnly DateOfBirth
);

public record DoctorDto(
    long Id,
    DateOnly DateOfBirth,
    long OfficeId,
    long CareerStartYear,
    string SpecializationName
);

public record ReceptionistDto(
    long Id,
    long OfficeId
);

public static class DtoConverters
{
    public static BaseAccountDto ToDto(this Account account)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new BaseAccountDto(
            account.Id,
            account.Email,
            account.PhoneNumber,
            account.FirstName,
            account.LastName,
            account.MiddleName,
            account.PhotoId
        );
    }

    public static PatientDto? ToDto(this Models.Patient? patient)
    {
        if (patient == null) return null;

        return new PatientDto(
            patient.Id,
            patient.DateOfBirth
        );
    }

    public static DoctorDto? ToDto(this Doctor? doctor)
    {
        if (doctor == null) return null;

        return new DoctorDto(
            doctor.Id,
            doctor.DateOfBirth,
            doctor.OfficeId,
            doctor.CareerStartYear,
            doctor.Specialization?.SpecializationName ?? "Unknown"
        );
    }

    public static ReceptionistDto? ToDto(this Receptionist? receptionist)
    {
        if (receptionist == null) return null;

        return new ReceptionistDto(
            receptionist.Id,
            receptionist.OfficeId
        );
    }
}