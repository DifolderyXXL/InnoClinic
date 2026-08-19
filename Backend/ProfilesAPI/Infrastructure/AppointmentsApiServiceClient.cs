using System.Net;
using MicroserviceApiKernel.Results;
using ProfilesAPI.Application;

namespace ProfilesAPI.Infrastructure;

public class AppointmentsApiServiceClient(HttpClient client) : IAppointmentsApiServiceClient
{
    public async Task<Result> DeleteAllUserAppointments(Guid userId, CancellationToken ct)
    {
        try
        {
            var response = await client.DeleteAsync($"Appointments/users/{userId}", ct);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            return response.StatusCode switch
            {
                HttpStatusCode.NotFound => Result.Success(),
                _ => Error.Failure("AppointmentApi.DeleteAppointmentsFailed", $"Failed to delete user appointments. Status: {response.StatusCode}")
            };
        }
        catch (Exception ex)
        {
            return Error.Failure("AppointmentApi.DeleteAppointmentsException", ex.Message);
        }
    }
}