namespace ProfilesAPI.Models;

public partial class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; }


    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string? MiddleName { get; set; }


    public long? PhotoId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }


    public virtual Patient? Patient { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual Receptionist? Receptionist { get; set; }
}

public class Patient
{
    public long Id { get; set; }
    public DateOnly DateOfBirth { get; set; }

    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; }
}

public class Receptionist
{
    public long Id { get; set; }
    public Guid AccountId { get; set; }
    public virtual Account Account { get; set; }
    public long OfficeId { get; set; }
}

public class Office
{
    public long Id { get; set; }
    public long? PhotoId { get; set; }

    public string City { get; set; }
    public string Street { get; set; }
    public string HouseNumber { get; set; }

    public string RegistryPhoneNumber { get; set; }
    public bool IsActive { get; set; }
}

public class Photo
{
    public long Id { get; set; }
    public string Url { get; set; }
}
