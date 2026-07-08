using Microsoft.OpenApi;

namespace MicroserviceApiKernel.Extensions;

public static class SecuritySchemeHelper
{
    public static OpenApiSecurityScheme GetOauth2SecurityScheme()
        => new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri("https://localhost:6001/connect/authorize"),
                    TokenUrl = new Uri("https://localhost:6001/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "api", "Api" },
                        { "openid", "Access the OpenID Connect user profile" },
                        { "email", "Access the user's email address" },
                        { "profile", "Access the user's profile" }
                    }
                }
            }
        };

    public static OpenApiSecurityRequirement GetOauth2SecurityRequirement(OpenApiDocument document)
        => new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("oauth2", document),
                new List<string> { "api", "profile", "email", "openid" }
            }
        };
}