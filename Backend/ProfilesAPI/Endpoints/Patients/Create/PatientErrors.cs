using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Endpoints.Patients.Create;

public static class PatientErrors
{
    public static Error AlreadyExists() => Error.Create(ErrorType.Conflict);
    public static Error NotFound() => Error.Create(ErrorType.NotFound);
}