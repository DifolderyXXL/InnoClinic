namespace ProfilesAPI.Endpoints.Doctors;

public record DoctorDto
{
    public Guid AccountId { get; init; }

    public string AccountFirstName { get; init; }
    public string AccountLastName { get; init; }
    public string? AccountMiddleName { get; init; }
    public Guid? AccountPhotoId { get; init; }
    public string? PhotoUrl { get; set; }

    public DateOnly DateOfBirth { get; set; }
    public long SpecializationId { get; init; }

    public string SpecializationName { get; init; }

    public string OfficeId { get; init; }
    public long CareerStartYear { get; init; }
}
