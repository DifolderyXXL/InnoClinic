using MicroserviceApiKernel.Results;

namespace ServicesAPI.Endpoints.Specializations.DeleteSpecialization;

public static class SpecializationErrors
{
    public static Error SpecializationNotFound() => Error.Create(ErrorType.NotFound);
}
