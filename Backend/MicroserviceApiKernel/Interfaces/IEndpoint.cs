using Microsoft.AspNetCore.Routing;

namespace MicroserviceApiKernel;

public interface IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder);
    double Version => 1.0;
    string[]? Tags => null;
}