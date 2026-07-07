using System.Reflection;
using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
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
        var microserviceAssembly = assembly ?? Assembly.GetEntryAssembly() ?? throw new InvalidOperationException("Called from unmanaged code");
        
        builder.AddServiceDefaults();
        builder.Services.AddHandlers(microserviceAssembly);

        builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        
        builder.AddOpenApiReversedThroughProxy(routeOnReversedProxy);
        builder.AddSwaggerDefaults();
        builder.AddAuthorizationDefaultsWithAspire();

        builder.Services.AddEndpoints(microserviceAssembly);
        builder.Services.AddApiAuthorizationPolicies();

    }
}