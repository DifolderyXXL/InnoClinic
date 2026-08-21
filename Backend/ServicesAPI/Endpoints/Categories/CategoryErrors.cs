using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Categories;

public static class CategoryErrors
{
    public static Error CategoryNotFound() => Error.Create(ErrorType.NotFound);
}
