using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Specializations;

public static class SpecializationErrors
{
    public static Error SpecializationNotFound() => Error.Create(ErrorType.NotFound);
}
