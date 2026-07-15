using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace MicroserviceApiKernel.Extensions;

public static class MicroserviceDefaultsExtension
{
    /// <summary>
    /// Register IEndpoints, CQRS handlers, Fluent Validation.
    /// Registers the openapi and swagger with authorization, and endpoint call through the reverse proxy @routeOnReversedProxy.
    /// Registers authorization policies.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="routeOnReversedProxy"></param>
    /// <param name="assembly"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public static void AddMicroserviceDefaults(this IHostApplicationBuilder builder, string routeOnReversedProxy, Assembly? assembly = null)
    {
        var microserviceAssembly = assembly 
                                   ?? Assembly.GetEntryAssembly() 
                                   ?? throw new InvalidOperationException("Called from unmanaged code");
        
        builder.AddServiceDefaults();
        
        builder.Services.AddHandlers(microserviceAssembly);
        builder.Services.AddEndpoints(microserviceAssembly);

        builder.Services.AddMicroserviceVersioning();
        
        builder.AddOpenApiReversedThroughProxy(routeOnReversedProxy);
        builder.AddSwaggerDefaults();
        
        builder.AddAuthorizationDefaultsWithAspire();

        builder.Services.AddApiAuthorizationPolicies();
    }
}