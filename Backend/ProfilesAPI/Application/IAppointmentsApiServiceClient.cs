using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Application;

public interface IAppointmentsApiServiceClient
{
    Task<Result> DeleteAllUserAppointments(Guid userId, CancellationToken ct);
}