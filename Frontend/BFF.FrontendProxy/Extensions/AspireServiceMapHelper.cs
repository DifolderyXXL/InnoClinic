

using Duende.Bff;
using Duende.Bff.Yarp;
using Yarp.ReverseProxy.Transforms;

public static class AspireServiceMapHelper
{
    public static IEndpointConventionBuilder MapAspireBffService(this IEndpointRouteBuilder app, IConfiguration configuration, string aspireServiceName, PathString localPath)
    {
        var baseRoute = localPath.Value!.TrimEnd('/');

        return app.MapRemoteBffApiEndpoint(localPath, new Uri($"http://{aspireServiceName}"), context =>
        {
            // Use the default BFF transformer (removes Cookie, removes local path, adds access token)
            DefaultBffYarpTransformerBuilders.DirectProxyWithAccessToken(baseRoute, context);
        });
    }
}
