using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Categories.DeleteCategory;

public static class CategoryErrors
{
    public static Error CategoryNotFound() => Error.Create(ErrorType.NotFound);
}
