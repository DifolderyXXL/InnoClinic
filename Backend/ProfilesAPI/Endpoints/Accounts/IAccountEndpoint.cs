using MicroserviceApiKernel;

namespace ProfilesAPI.Endpoints.Accounts;

public interface IAccountEndpoint : IEndpoint
{
    string[] IEndpoint.Tags => ["Accounts"];
}