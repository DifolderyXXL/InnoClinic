namespace ProfilesAPI.Endpoints.Accounts;

public record AccountDto
{
    public Guid Id { get; init; }
    public string Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsEmailVerified { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string? MiddleName { get; init; }
    public Guid? PhotoId { get; init; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
