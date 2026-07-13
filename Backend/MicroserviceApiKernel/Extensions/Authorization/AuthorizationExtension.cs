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
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
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
        builder.Services.AddSingleton<IAuthorizationHandler, ScopeRequirementHandler>();
    }

    public static void UseAuthorizationDefaultsWithAspire(this IApplicationBuilder app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public static void AddApiAuthorizationPolicies(this IServiceCollection services)
    {
        string[] roleClaimNames = ["role", System.Security.Claims.ClaimTypes.Role];
        services.AddAuthorizationBuilder()
            .AddPolicy(RolePolicy.Client, policy =>
                policy
                    .AddRequirements(
                        new RoleRequirement(["client"], roleClaimNames),
                        new ScopeRequirement("api"))
                    )
            .AddPolicy(RolePolicy.DoctorOrReceptionist, policy =>
                policy
                    .AddRequirements(
                        new RoleRequirement(["doctor", "receptionist"], roleClaimNames),
                        new ScopeRequirement("api"))
                    )
            .AddPolicy(RolePolicy.Receptionist, policy =>
                policy
                    .AddRequirements(
                        new RoleRequirement(["receptionist"], roleClaimNames),
                        new ScopeRequirement("api"))
                    )
            .AddPolicy(RolePolicy.Doctor, policy =>
                policy
                    .AddRequirements(
                        new RoleRequirement(["doctor"], roleClaimNames),
                        new ScopeRequirement("api"))
            );

    }

    public static void AddIdentityAuthorizationPolicies(this IServiceCollection services)
    {        
        services.AddAuthentication()
            .AddJwtBearer("LocalM2M", options =>
            {
                options.Authority = "https://localhost:6001";
                options.RequireHttpsMetadata = true;
                
                options.MapInboundClaims = false;
        
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://localhost:6001",
                    
                    ValidateAudience = false 
                };
            });
        services.AddAuthorizationBuilder()
            .AddPolicy(RolePolicy.IdentityServer, policy =>
                policy
                    .AddAuthenticationSchemes("LocalM2M")
                    .AddRequirements(
                        new ScopeRequirement("identity"))
                    );
    }
}
