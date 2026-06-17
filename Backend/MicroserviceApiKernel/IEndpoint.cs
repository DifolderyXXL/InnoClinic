using Microsoft.AspNetCore.Routing;

namespace MicroserviceApiKernel;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder);
}
