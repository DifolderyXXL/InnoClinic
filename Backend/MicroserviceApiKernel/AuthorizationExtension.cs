using System;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace MicroserviceApiKernel;

public static class AuthorizationExtension
{
    public static void AddAuthorizationDefaultsWithAspire(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpLogging(logging =>
        {
            logging.LoggingFields = HttpLoggingFields.RequestHeaders | HttpLoggingFields.ResponseStatusCode;
            logging.RequestHeaders.Add("Authorization");
        });

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration.DiscoverHttps("IdentityServer");
            options.Audience = "api";
            options.IncludeErrorDetails = true;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
            };
        });

        builder.Services.AddSingleton<IAuthorizationHandler, RoleRequirementHandler>();
    }

    public static void UseAuthorizationDefaultsWithAspire(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(RolePolicy.Client, policy =>
                policy
                    .AddRequirements(
                        new RoleRequirement("client"),
                        new ScopeRequirement("api"))
                    );
    }
}


public static class RolePolicy
{
    public const string Client = "ClientPolicyName";
}