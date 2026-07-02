using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Services.UpdateService;

public static class CategoryErrors
{
    public static Error CategoryNotFound() => Error.Create(ErrorType.NotFound);
}
