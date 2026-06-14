using BFF.FrontendProxy;
using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpLogging;
using Yarp.ReverseProxy.Forwarder;
using System.Net;
using System.Diagnostics;

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

builder.Services.AddBff()
    .AddRemoteApis();

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
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = builder.Configuration["services:IdentityServer:https:0"]
                      ?? builder.Configuration["services:IdentityServer:http:0"]
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
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.MapDefaultEndpoints();
app.UseHttpLogging();

app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

// app.MapAspireBffService(builder.Configuration, "ProfilesAPI", "/api/profiles")
//     .WithAccessToken(RequiredTokenType.User);

if (config.Apis.Any())
{
    foreach (var api in config.Apis)
    {
        var remoteUri = new Uri(api.RemoteUrl!);
        app.MapRemoteBffApiEndpoint(api.PathMatch, remoteUri).WithAccessToken(api.RequiredToken);
    }
}

// =========================================================================
// Все запросы, которые не подошли под API, улетают на дев-сервер Vite (5173)
// =========================================================================
app.MapGet("/{*rest}", async (IHttpForwarder forwarder, HttpContext context) =>
{
    await ForwardAllRequestsToNpmDevServer(forwarder, context, "http://localhost:5173");
});

app.Run();

static async Task ForwardAllRequestsToNpmDevServer(IHttpForwarder forwarder, HttpContext context, string destinationPrefix)
{
    var httpClient = new HttpMessageInvoker(
        new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
        }
    );

    var requestConfig = new ForwarderRequestConfig { };

    if (context.Request.Path == "/")
    {
        context.Request.Path = "/index.html";
    }

    var error = await forwarder.SendAsync(
        context,
        destinationPrefix,
        httpClient,
        requestConfig,
        HttpTransformer.Default
    );
}