using MicroserviceApiKernel;

namespace ProfilesAPI.Endpoints.Doctors;

public interface IDoctorEndpoint : IEndpoint
{
    string[] IEndpoint.Tags => ["Doctors"];
}