namespace ProfilesAPI.Models;

public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public Photo? Photo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Patient
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public Account Account { get; set; }
}

public class Receptionist
{
    public long Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }

    public Account Account { get; set; }
    public Office Office { get; set; }
}

public class Office
{
    public long Id { get; set; }
    public Photo? Photo { get; set; }
    public string RegistryPhoneNumber { get; set; }
    public bool IsActive { get; set; }
}

public class Photo
{
    public long Id { get; set; }
    public string Url { get; set; }
}