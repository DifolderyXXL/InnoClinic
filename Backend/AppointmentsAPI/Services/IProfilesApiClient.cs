using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;

namespace AppointmentsAPI.Services;

public record ValidateAppointmentContextRequest(
    Guid DoctorId, 
    Guid PatientId, 
    string OfficeId);

public record ValidateAppointmentContextResponse(
    bool IsValid, 
    string DoctorFullName, 
    string PatientFullName, 
    string Email,
    long DoctorSpecializationId);

public interface IProfilesApiClient
{
    Task<Result<ValidateAppointmentContextResponse>> ValidateAppointmentContextAsync(
        ValidateAppointmentContextRequest request, 
        CancellationToken ct = default);
}

public class ProfilesApiClient(HttpClient httpClient) : IProfilesApiClient
{
    public async Task<Result<ValidateAppointmentContextResponse>> ValidateAppointmentContextAsync(
        ValidateAppointmentContextRequest request, 
        CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/v1/profiles/validate-profile", 
            request, 
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.ReadErrorAsync(ct);
            return error;
        }

        var result = await response.Content.ReadFromJsonAsync<ValidateAppointmentContextResponse>(cancellationToken: ct);

        if (result == null)
        {
            return new Error("NullResponse", "Received null response from Profiles API.", ErrorType.Problem);
        }

        return Result.Success(result);
    }
}

public record ServiceDto(
    long Id,
    string ServiceName,
    decimal Price,
    bool IsActive,
    int SlotLength,
    long CategoryId,
    string CategoryName,
    long SpecializationId,
    string SpecializationName);

public interface IServicesApiClient
{
    public Task<Result<ServiceDto>> GetService(long serviceId, CancellationToken ct = default);
}

public class ServicesApiClient(HttpClient httpClient) : IServicesApiClient
{
    public async Task<Result<ServiceDto>> GetService(long serviceId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(
            $"api/v1/services/{serviceId}", 
            ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.ReadErrorAsync(ct);
            return error;
        }

        var service = await response.Content.ReadFromJsonAsync<ServiceDto>(cancellationToken: ct);

        if (service is null)
        {
            return Result.Failure<ServiceDto>(
                new Error("NullResponse", "Received null response from Services API.",  ErrorType.Problem));
        }

        return Result.Success(service);
    }
}
