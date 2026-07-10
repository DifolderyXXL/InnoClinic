namespace ProfilesAPI.Endpoints.Receptionists;

public record ReceptionistDto
{
    public long Id { get; init; }
    public Guid AccountId { get; init; }
    public string AccountFirstName { get; init; }
    public string AccountLastName { get; init; }
    public string? AccountMiddleName { get; init; }
    public string AccountEmail { get; init; }
    public long OfficeId { get; init; }
}
