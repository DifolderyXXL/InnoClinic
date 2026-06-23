using BFF.FrontendProxy;
using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpLogging;
using Yarp.ReverseProxy.Forwarder;
using System.Net;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2;
    });
});

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
builder.Services.AddOpenApi(options =>
        options.AddDocumentTransformer<BearerSecuritySchemeTransformer>()
    );



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
var profiles = app.Configuration.DiscoverAny("ProfilesAPI");
var offices = app.Configuration.DiscoverAny("OfficesAPI");
app.MapSwaggerUI(setupAction: options =>
{

    options.SwaggerEndpoint(profiles + "/openapi/v1.json", "Profiles API v1");
    options.SwaggerEndpoint(offices + "/openapi/v1.json", "Offices API v1");


    options.ConfigObject.AdditionalItems["withCredentials"] = true;
    options.UseRequestInterceptor("function(request){ request.headers['X-CSRF'] = '1';return request;}");
})
.AllowAnonymous();


app.MapAspireBffService(builder.Configuration, "ProfilesAPI", "/api/profiles")
    .WithAccessToken(RequiredTokenType.User).DisableAntiforgery();

// if (config.Apis.Any())
// {
//     foreach (var api in config.Apis)
//     {
//         var discovered = builder.Configuration.DiscoverAny(new Uri(api.RemoteUrl!).Host)
//             ?? api.RemoteUrl!;

//         var remoteUri = new Uri(discovered);

//         app.MapRemoteBffApiEndpoint(api.PathMatch, remoteUri)
//             .WithAccessToken(api.RequiredToken);
//     }
// }


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

    var requestConfig = new ForwarderRequestConfig
    {
        Version = HttpVersion.Version11,
        VersionPolicy = HttpVersionPolicy.RequestVersionExact
    };

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


internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // "bearer" refers to the header name here
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token"
                }
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = (IDictionary<string, IOpenApiSecurityScheme>?)requirements;
        }
    }
}