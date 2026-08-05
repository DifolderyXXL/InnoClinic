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
            
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RoleClaimType = "role",
                NameClaimType = "sub"
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
        services.AddAppAuthorization();
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
    
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(Permissions.Accounts.ReadOwn, p => p.RequireRole(Roles.Client, Roles.Doctor, Roles.Receptionist))
            .AddPolicy(Permissions.Accounts.Read, p => p.RequireRole(Roles.Receptionist))
            .AddPolicy(Permissions.Accounts.Manage, p => p.RequireRole(Roles.Receptionist))

            .AddPolicy(Permissions.Doctors.Read, p => p.RequireRole(Roles.Client, Roles.Doctor, Roles.Receptionist))
            .AddPolicy(Permissions.Doctors.Manage, p => p.RequireRole(Roles.Receptionist))

            .AddPolicy(Permissions.Patients.Read, p => p.RequireRole(Roles.Doctor, Roles.Receptionist))
            .AddPolicy(Permissions.Patients.Manage, p => p.RequireRole(Roles.Receptionist))
            .AddPolicy(Permissions.Patients.ManageOwn, p => p.RequireRole(Roles.Client))

            .AddPolicy(Permissions.Offices.Read, p => p.RequireAssertion(_ => true))
            .AddPolicy(Permissions.Offices.Manage, p => p.RequireRole(Roles.Receptionist))

            .AddPolicy(Permissions.Services.Read, p => p.RequireAssertion(_ => true))
            .AddPolicy(Permissions.Services.Manage, p => p.RequireRole(Roles.Receptionist))

            .AddPolicy(Permissions.Specializations.Read, p => p.RequireAssertion(_ => true))
            .AddPolicy(Permissions.Specializations.Manage, p => p.RequireRole(Roles.Receptionist))

            .AddPolicy(Permissions.Schedules.ReadOwn, p => p.RequireRole(Roles.Doctor))
            .AddPolicy(Permissions.Schedules.Read, p => p.RequireRole(Roles.Client, Roles.Doctor, Roles.Receptionist))
            .AddPolicy(Permissions.Schedules.Manage, p => p.RequireRole(Roles.Receptionist))

            .AddPolicy(Permissions.Appointments.ReadOwn, p => p.RequireRole(Roles.Client, Roles.Doctor))
            .AddPolicy(Permissions.Appointments.Read, p => p.RequireRole(Roles.Doctor, Roles.Receptionist))
            .AddPolicy(Permissions.Appointments.Manage, p => p.RequireRole(Roles.Client, Roles.Receptionist))

            .AddPolicy(Permissions.MedicalResults.ReadOwn, p => p.RequireRole(Roles.Client))
            .AddPolicy(Permissions.MedicalResults.Manage, p => p.RequireRole(Roles.Doctor))
            .AddPolicy(Permissions.MedicalResults.Read, p => p.RequireRole(Roles.Doctor))

            .AddPolicy(Permissions.Photos.Read, p => p.RequireAssertion(_ => true))
            .AddPolicy(Permissions.Photos.Manage, p => p.RequireRole(Roles.Client, Roles.Doctor, Roles.Receptionist));

        return services;
    }
}
public static class Roles
{
    public const string Client = "client";
    public const string Doctor = "doctor";
    public const string Receptionist = "receptionist";
}
public static class Permissions
{
    public static class Accounts
    {
        public const string ReadOwn = "accounts.read_own";
        public const string Read = "accounts.read";
        public const string Manage = "accounts.manage";
    }

    public static class Doctors
    {
        public const string Read = "doctors.read";
        public const string Manage = "doctors.manage";
    }

    public static class Patients
    {
        public const string Read = "patients.read";
        public const string Manage = "patients.manage";
        public const string ManageOwn = "patients.manage_own";
    }

    public static class Offices
    {
        public const string Read = "offices.read";
        public const string Manage = "offices.manage";
    }

    public static class Services
    {
        public const string Read = "services.read";
        public const string Manage = "services.manage";
    }

    public static class Specializations
    {
        public const string Read = "specializations.read";
        public const string Manage = "specializations.manage";
    }

    public static class Schedules
    {
        public const string ReadOwn = "schedules.read_own";
        public const string Read = "schedules.read";
        public const string Manage = "schedules.manage";
    }

    public static class Appointments
    {
        public const string ReadOwn = "appointments.read_own";
        public const string Read = "appointments.read";
        public const string Manage = "appointments.manage";
    }

    public static class MedicalResults
    {
        public const string ReadOwn = "medical_results.read_own";
        public const string Read = "medical_results.read";
        public const string Manage = "medical_results.manage";
    }

    public static class Photos
    {
        public const string Read = "photos.read";
        public const string Manage = "photos.manage";
    }
}