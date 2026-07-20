namespace DocumentsAPI.Application;

public record UserFullName(string FirstName, string LastName, string? MiddleName)
{
    public override string ToString()
    {
        var name = $"{FirstName} {LastName}";
        if (MiddleName != null) name += $" {MiddleName}";
        
        return name;
    }
}