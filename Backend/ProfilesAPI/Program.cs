using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using ProfilesAPI.Consumers;
using ProfilesAPI.Data;
using ServiceDefaults;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ProfilesAPI.Application;
using ProfilesAPI.Infrastructure;


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
var builder = WebApplication.CreateBuilder(args);

builder.AddMicroserviceDefaults("/profiles", typeof(Program).Assembly);

builder.Services.AddDbContext<ProfilesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("profilesSqlServer")));

builder.AddCredentialsClient("identityclient");

builder.Services.AddSingleton<IPhotoUrlFactory>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var gatewayBaseUrl = config.DiscoverHttps("BffProxy");

        if (string.IsNullOrWhiteSpace(gatewayBaseUrl))
        {
            throw new InvalidOperationException("BffProxy URL not found.");
        }
        
        return new DocumentsPhotoUrlFactory(gatewayBaseUrl);
    }
);

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<SpecializationUpdatedEventConsumer>();
    x.AddConsumer<SpecializationCreatedEventConsumer>();
    x.AddConsumer<SpecializationDeletedEventConsumer>();
    
    x.AddDelayedMessageScheduler();

    x.UseOutbox<ProfilesDbContext>(o =>
    {
        o.UseSqlServer();
    });
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));

        cfg.UseDelayedMessageScheduler();

        cfg.ConfigureEndpoints(context);
    });
});



var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapSwaggerDefaults();
}



app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);

app.UseHttpsRedirection();

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();

const string TestGroupTag = "Test Endpoints";
app.MapGet("/my-profile", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value
              ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var email = user.FindFirst("email")?.Value
             ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    return Results.Ok(new { Message = "Okey, we have profile", UserId = userId, Email = email });
}).RequireAuthorization().WithTags(TestGroupTag);

app.MapGet("/client-only", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value
              ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var email = user.FindFirst("email")?.Value
             ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    return Results.Ok(new { Message = "You are only client, ok!", UserId = userId, Email = email });
}).RequireAuthorization(RolePolicy.Client).WithTags(TestGroupTag);

app.MapGet("/get-headers", (HttpContext context) =>
{
    return Results.Ok(context.Request.Headers.Select(x => $"{x.Key}: {x.Value}").ToArray());
}).WithTags(TestGroupTag);



app.Run();