using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Services.UpdateService;

public static class SpecializationErrors
{
    public static Error SpecializationNotFound() => Error.Create(ErrorType.NotFound);
}
