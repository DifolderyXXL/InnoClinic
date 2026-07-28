using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public static class DoctorErrors
{
    public static Error AlreadyExists() => Error.Create(ErrorType.Conflict);

    public static Error DoctorNotFound() => Error.Create(ErrorType.NotFound);
    public static Error DoctorNotFoundInOffice() => Error.Create(ErrorType.Validation);
}


public static class SpecializationErrors
{
    public static Error SpecializationNotFound() => Error.Create(ErrorType.NotFound);
}