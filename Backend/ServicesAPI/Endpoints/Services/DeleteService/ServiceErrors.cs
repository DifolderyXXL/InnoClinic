using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Services.DeleteService;

public static class ServiceErrors
{
    public static Error ServiceNotFound() => Error.Create(ErrorType.NotFound);
}
