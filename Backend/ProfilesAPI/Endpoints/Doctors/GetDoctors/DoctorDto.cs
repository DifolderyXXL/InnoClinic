namespace ProfilesAPI.Endpoints.Doctors;

public record DoctorDto
{
    public long Id { get; init; }

    public string AccountFirstName { get; init; }
    public string AccountLastName { get; init; }
    public string? AccountMiddleName { get; init; }
    public long? AccountPhotoId { get; init; }

    public DateOnly DateOfBirth { get; set; }
    public long SpecializationId { get; init; }

    public string SpecializationSpecializationName { get; init; }

    public long OfficeId { get; init; }
    public long CareerStartYear { get; init; }
}
