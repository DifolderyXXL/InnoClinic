using System;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
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
}