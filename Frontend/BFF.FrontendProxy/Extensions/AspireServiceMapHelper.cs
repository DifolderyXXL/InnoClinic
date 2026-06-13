

using Duende.Bff.Yarp;
using Yarp.ReverseProxy.Transforms;

public static class AspireServiceMapHelper
{
    public static IEndpointConventionBuilder MapAspireBffService(this IEndpointRouteBuilder app, IConfiguration configuration, string aspireServiceName, PathString localPath)
    {

        string apiLink = configuration[$"services:{aspireServiceName}:http:0"]
                      ?? configuration[$"services:{aspireServiceName}:https:0"]
                      ?? throw new InvalidOperationException($"{aspireServiceName} service URL not found.");

        var baseRoute = localPath.Value!.TrimEnd('/');

        return app.MapRemoteBffApiEndpoint(localPath, new Uri(apiLink));
    }
}
