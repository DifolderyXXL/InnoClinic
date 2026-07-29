using BFF.FrontendProxy;
using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpLogging;
using Yarp.ReverseProxy.Forwarder;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json.Nodes;
using MicroserviceApiKernel.Extensions;


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
app.UseHttpLogging();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();


app.MapGet("/login", async (HttpContext context) =>
{
    await context.ChallengeAsync("oidc", new AuthenticationProperties
    {
        RedirectUri = "/swagger/index.html"
    });
});

var defenitions = app.Configuration.GetSection("BFF:Microservices").Get<List<MicroserviceDefenition>>() ?? [];

app.MapSwaggerUI(setupAction: options =>
{
    foreach (var definition in defenitions)
    {
        var host = app.Configuration.DiscoverAny(definition.Name);
        foreach (var version in definition.SupportedVersions)
        {
            options.SwaggerEndpoint(
                $"{host}/openapi/{version}.json", 
                $"{definition.Name} {version}"
            );
        }
    }

    options.ConfigObject.AdditionalItems["withCredentials"] = true;
    options.UseRequestInterceptor("function(request){ request.headers['X-CSRF'] = '1';return request;}");
})
.AllowAnonymous();


foreach (var definition in defenitions)
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