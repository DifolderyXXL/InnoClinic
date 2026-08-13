using System.Net.Http.Json;
using MicroserviceApiKernel.Extensions;
using MicroserviceApiKernel.Results;

namespace NotificationService.Worker.Services;

public class AppointmentApiClient(HttpClient client)
{
    public async Task<Result<AppointmentInformationDto>> GetAppointmentInfoAsync(Guid id, CancellationToken ct = default)
    {
        var response = await client.GetAsync($"Appointments/{id}/info", ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.ReadErrorAsync(ct);
            return error;
        }

        var result = await response.Content.ReadFromJsonAsync<AppointmentInformationDto>(cancellationToken: ct);

        if (result is null)
        {
            return new Error("NullResponse", "Received null response from Appointment API.", ErrorType.Problem);
        }

        return Result.Success(result);
    }
}

public class AppointmentInformationDto
{
    public string PatientEmail { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public TimeSpan? BeginTime { get; init; }
    public TimeSpan? EndTime { get; init; }
}