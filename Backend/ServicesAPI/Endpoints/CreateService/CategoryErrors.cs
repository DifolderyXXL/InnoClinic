using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.CreateService;

public static class CategoryErrors
{
    public static Error CategoryNotFound() => Error.Create(ErrorType.NotFound);
}
