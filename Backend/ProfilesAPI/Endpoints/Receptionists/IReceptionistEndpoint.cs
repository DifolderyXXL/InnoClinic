using MicroserviceApiKernel;

namespace ProfilesAPI.Endpoints.Receptionists;

public interface IReceptionistEndpoint : IEndpoint
{
    string[] IEndpoint.Tags => ["Receptionists"];
}