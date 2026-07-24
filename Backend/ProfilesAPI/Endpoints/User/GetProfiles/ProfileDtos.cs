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

public record OnlyPatientDto(
    long Id,
    DateOnly DateOfBirth
);

public record OnlyDoctorDto(
    long Id,
    DateOnly DateOfBirth,
    string OfficeId,
    long CareerStartYear,
    string SpecializationName
);

public record OnlyReceptionistDto(
    long Id,
    string OfficeId
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

    public static OnlyPatientDto? ToDto(this Models.Patient? patient)
    {
        if (patient == null) return null;

        return new OnlyPatientDto(
            patient.Id,
            patient.DateOfBirth
        );
    }

    public static OnlyDoctorDto? ToDto(this Doctor? doctor)
    {
        if (doctor == null) return null;

        return new OnlyDoctorDto(
            doctor.Id,
            doctor.DateOfBirth,
            doctor.OfficeId,
            doctor.CareerStartYear,
            doctor.Specialization?.SpecializationName ?? "Unknown"
        );
    }

    public static OnlyReceptionistDto? ToDto(this Receptionist? receptionist)
    {
        if (receptionist == null) return null;

        return new OnlyReceptionistDto(
            receptionist.Id,
            receptionist.OfficeId
        );
    }
}