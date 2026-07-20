using FluentValidation;
using FluentValidation.Results;
using MicroserviceApiKernel.CQRS;
using MicroserviceApiKernel.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Sockets;
using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;

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
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();
        
        var v1Group = app.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(1, 0);
        var v2Group = app.MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(2, 0);
        
        var endpoints = app.Services.GetServices<IEndpoint>();
        foreach (var endpoint in endpoints)
        {
            var versionGroup = endpoint.Version switch
            {
                2.0 => v2Group,
                1.0 => v1Group,
                _ => throw new Exception("Api Version is not valid.")
            };

            var targetGroup = endpoint.Tags == null
                ? versionGroup
                : versionGroup.MapGroup(string.Empty)
                    .WithTags(endpoint.Tags);
            
            endpoint.MapEndpoint(targetGroup);
        }
    }
}
