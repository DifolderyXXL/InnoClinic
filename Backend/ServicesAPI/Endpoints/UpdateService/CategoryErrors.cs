using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.UpdateService;

public static class CategoryErrors
{
    public static Error CategoryNotFound() => Error.Create(ErrorType.NotFound);
}
