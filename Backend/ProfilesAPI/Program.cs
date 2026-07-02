using Duende.AccessTokenManagement;
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


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHandlers(typeof(Program).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddOpenApiReversedThroughProxy("/api/profiles");

builder.AddSwaggerDefaults();
builder.AddAuthorizationDefaultsWithAspire();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextPool<ProfilesDbContext>(options => options.UseSqlite(connectionString).UseLazyLoadingProxies());

builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddApiAuthorizationPolicies();

builder.Services.AddClientCredentialsTokenManagement()
    .AddClient("identityclient", client =>
    {
        client.TokenEndpoint = new Uri("https://localhost:6001/connect/token");

        client.ClientId = ClientId.Parse("m2m");
        client.ClientSecret = ClientSecret.Parse("secret");
        client.Scope = Duende.AccessTokenManagement.Scope.Parse("identity");
    });

builder.Services.AddClientCredentialsHttpClient("client", ClientCredentialsClientName.Parse("client"), client =>
{
    client.BaseAddress = new Uri("https://localhost:6001/api");
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;

    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<SpecializationUpdatedEventConsumer>();
    x.AddConsumer<SpecializationCreatedEventConsumer>();
    x.AddConsumer<SpecializationDeletedEventConsumer>();
    
    x.AddDelayedMessageScheduler();

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
