using MicroserviceApiKernel;

namespace ProfilesAPI.Endpoints.Patients;

public interface IPatientEndpoint : IEndpoint
{
    string[] IEndpoint.Tags => ["Patients"];
}