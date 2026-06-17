using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace MicroserviceApiKernel.Extensions;

public static class EndpointExtension
{
    public static IServiceCollection AddEndpoints(this IServiceCollection builder, Assembly assembly)
    {

        var descriptors = assembly.GetTypes().Where(t => typeof(IEndpoint).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(x => ServiceDescriptor.Describe(typeof(IEndpoint), x, ServiceLifetime.Transient));

        builder.TryAddEnumerable(descriptors);

        return builder;
    }

    public static void MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }
    }
}
