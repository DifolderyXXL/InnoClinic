namespace ProfilesAPI.Endpoints.Patients;

public record PatientDto
{
    public long Id { get; init; }
    public Guid AccountId { get; init; }
    public DateOnly DateOfBirth { get; init; }
    public string AccountFirstName { get; init; }
    public string AccountLastName { get; init; }
    public string? AccountMiddleName { get; init; }
    public string AccountEmail { get; init; }
}
