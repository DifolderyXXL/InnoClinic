using MicroserviceApiKernel.Results;

namespace OfficesApi.Endpoints.GetOffices;

public static class OfficeErrors
{
    public static Error NotFound() => Error.Create(ErrorType.NotFound);
}
