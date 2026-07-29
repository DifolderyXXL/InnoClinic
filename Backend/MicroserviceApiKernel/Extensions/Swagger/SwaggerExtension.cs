using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MicroserviceApiKernel.Extensions;

public static class SwaggerExtension
{
    public static void AddOpenApiReversedThroughProxy(this IHostApplicationBuilder builder, string routeOnProxy, Action<Microsoft.AspNetCore.OpenApi.OpenApiOptions>? configureOptions = default)
    {
        builder.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<OidcSecuritySchemeTransformer>();
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Servers.Clear();
                document.Servers.Add(new()
                {
                    Url = "https://localhost:5001" + routeOnProxy
                });
                return Task.CompletedTask;
            });

            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (context.JsonTypeInfo.Type == typeof(TimeSpan))
                {
                    schema.Type = JsonSchemaType.String;
                    schema.Format = "duration";
                    schema.Example = JsonValue.Create("00:30:00");
                    schema.Pattern = null;
                }
                return Task.CompletedTask;
            });

            options.AddOperationTransformer<OpenApiRolesOperationTransformer>();

            configureOptions?.Invoke(options);
        });
    }
    public static void AddSwaggerDefaults(this IHostApplicationBuilder builder, Action<Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions> setupAction = null)
    {
        builder.Services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(i => i.FullName?.Replace('+', '_'));
            
            options.AddSecurityDefinition("oauth2", SecuritySchemeHelper.GetOauth2SecurityScheme());
            options.AddSecurityRequirement(SecuritySchemeHelper.GetOauth2SecurityRequirement);
            
            setupAction?.Invoke(options);
        });
    }
    public static IEndpointConventionBuilder MapSwaggerDefaults(this IEndpointRouteBuilder app, Action<SwaggerUIOptions>? setupOptions = null)
    {
        var configuration = app.ServiceProvider.GetRequiredService<IConfiguration>();

        app.MapSwagger();
        return app.MapSwaggerUI(setupAction: options =>
        {
            options.UseRequestInterceptor("function(request){ request.headers['X-CSRF'] = '1';request.credentials = 'include';return request;}");
            options.OAuthUsePkce();

            options.OAuthClientId(configuration["oauth:clientid"]);
            options.OAuthClientSecret(configuration["oauth:secret"]);

            setupOptions?.Invoke(options);
        });
    }
}public class OpenApiRolesOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation, 
        OpenApiOperationTransformerContext context, 
        CancellationToken cancellationToken)
    {
        var authorizeData = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IAuthorizeData>()
            .ToList();

        if (authorizeData.Count == 0) return Task.CompletedTask;

        var policies = authorizeData
            .Select(a => a.Policy)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        var roles = authorizeData
            .Select(a => a.Roles)
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .SelectMany(r => r!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Distinct()
            .ToList();

        operation.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        if (policies.Count > 0)
        {
            operation.Extensions["x-policies"] = new JsonOpenApiExtension(policies);
        }

        if (roles.Count > 0)
        {
            operation.Extensions["x-roles"] = new JsonOpenApiExtension(roles);
        }

        return Task.CompletedTask;
    }
}

public class JsonOpenApiExtension : IOpenApiExtension
{
    private readonly object _data;

    public JsonOpenApiExtension(object data)
    {
        _data = data;
    }

    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        var json = JsonSerializer.Serialize(_data);
        writer.WriteRaw(json);
    }
}