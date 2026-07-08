using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace MicroserviceApiKernel.Extensions;

internal sealed class OidcSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["oauth2"] = SecuritySchemeHelper.GetOauth2SecurityScheme()
        };

        document.Security = new List<OpenApiSecurityRequirement>
        {
            SecuritySchemeHelper.GetOauth2SecurityRequirement(document)
        };

        return Task.CompletedTask;
    }
}