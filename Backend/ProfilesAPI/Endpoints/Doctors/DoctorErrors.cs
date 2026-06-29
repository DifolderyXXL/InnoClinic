using MicroserviceApiKernel.Results;

namespace ProfilesAPI.Endpoints.Doctors.GetDoctorById;

public static class DoctorErrors
{
    public static Error DoctorNotFound() => Error.Create(ErrorType.NotFound);
}


public static class SpecializationErrors
{
    public static Error SpecializationNotFound() => Error.Create(ErrorType.NotFound);
}