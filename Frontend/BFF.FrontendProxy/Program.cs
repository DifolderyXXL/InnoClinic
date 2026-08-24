using BFF.FrontendProxy;
using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpLogging;
using Yarp.ReverseProxy.Forwarder;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json.Nodes;
using BFF.FrontendProxy.Consumers;
using BFF.FrontendProxy.Middlewares;
using BFF.FrontendProxy.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath
                          | HttpLoggingFields.RequestMethod
                          | HttpLoggingFields.RequestHeaders
                          | HttpLoggingFields.ResponseStatusCode;
});

builder.AddServiceDefaults();

builder.Services.AddHttpForwarder();
builder.Services.AddServiceDiscovery();


builder.Services.AddBff()
    .AddRemoteApis();
builder.Services.AddOpenApi();

builder.AddRedisClient(connectionName: "cache");

builder.Services.AddScoped<IRevokedUserRepository, RedisRevokedUserRepository>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserDeletedBlockingConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));

        cfg.ConfigureEndpoints(context);
    });
});

Configuration config = new();
builder.Configuration.Bind("BFF", config);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "bff-local-session";
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = builder.Configuration.DiscoverHttps("IdentityServer")
                      ?? config.Authority;

        options.ClientId = config.ClientId;
        options.ClientSecret = config.ClientSecret;
        options.ResponseType = "code";
        options.ResponseMode = "query";
        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.SaveTokens = true;

        options.Scope.Clear();
        foreach (var scope in config.Scopes)
        {
            options.Scope.Add(scope);
        }

        options.TokenValidationParameters = new()
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };
        
        options.Events.OnRedirectToIdentityProvider = context =>
        {
            if (context.Request.Query.TryGetValue("acr_values", out var acrValues))
            {
                context.ProtocolMessage.AcrValues = acrValues;
            }

            if (context.Request.Query.TryGetValue("prompt", out var prompt))
            {
                context.ProtocolMessage.Prompt = prompt;
            }
            
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseHttpLogging();
}

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

app.UseMiddleware<DeletedUserBarrierMiddleware>();


app.MapGet("/login", async (HttpContext context) =>
{
    await context.ChallengeAsync("oidc", new AuthenticationProperties
    {
        RedirectUri = "/swagger/index.html"
    });
});

var definitions = app.Configuration.GetSection("BFF:Microservices").Get<List<MicroserviceDefenition>>() ?? [];

if (app.Environment.IsDevelopment())
{
    app.UseBffGlobalSwagger(definitions);
}

foreach (var definition in definitions)
{
    app.MapAspireBffService(builder.Configuration, definition.Name, definition.Path)
        .WithAccessToken(RequiredTokenType.User)
        .AsBffApiEndpoint();
}


app.UseWebSockets();
var realViteAddress = app.Configuration.DiscoverAny("vite-frontend") ?? throw new Exception("Frontend address is not defined.");
// =========================================================================
// Все запросы, которые не подошли под API, улетают на дев-сервер Vite (5173)
// =========================================================================
app.MapGet("/{*rest}", async (IHttpForwarder forwarder, HttpContext context) =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\": \"API endpoint not found or incorrect version specified.\"}");
        return;
    }
    
    await ViteDevServerProxy.ForwardRequestAsync(forwarder, context, realViteAddress);
});

app.Run();

public class MicroserviceDefenition
{
    public string Name { get; set; }
    public string Path { get; set; }
    public string[] SupportedVersions { get; set; }
}