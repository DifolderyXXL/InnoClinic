namespace OfficesApi.Endpoints.GetOffices;

public record OfficeDto(string Id,
    long? PhotoId,
    string City,
    string Street,
    string HouseNumber,
    string? OfficeNumber,
    string RegistryPhoneNumber,
    bool IsActive);