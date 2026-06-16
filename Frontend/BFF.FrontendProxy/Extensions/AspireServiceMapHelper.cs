

using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Yarp.ReverseProxy.Transforms;

public static class AspireServiceMapHelper
{
    public static IEndpointConventionBuilder MapAspireBffService(this IEndpointRouteBuilder app, IConfiguration configuration, string aspireServiceName, PathString localPath)
    {

        string apiLink = configuration.DiscoverAny(aspireServiceName)
                      ?? throw new InvalidOperationException($"{aspireServiceName} service URL not found.");

        return app.MapRemoteBffApiEndpoint(localPath, new Uri(apiLink));
    }
}
