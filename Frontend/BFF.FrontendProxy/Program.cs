using BFF.FrontendProxy;
using Duende.Bff;
using Duende.Bff.AccessTokenManagement;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpLogging;
using ServiceDefaults;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestPath
                          | HttpLoggingFields.RequestMethod
                          | HttpLoggingFields.RequestHeaders // Увидишь, долетает ли кука
                          | HttpLoggingFields.ResponseStatusCode;
});

builder.AddServiceDefaults();

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
        options.Cookie.Name = "__Host-bff";
        options.Cookie.SameSite = SameSiteMode.Strict;
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = config.Authority;
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

//app.UseCors(PolicyConstants.FRONTEND_CORS_POLICY);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseBff();

app.UseAuthorization();

app.MapGet("/hello-world", () => "hello-world")
  .AsBffApiEndpoint();

//app.MapBffManagementEndpoints();

app.MapAspireBffService(builder.Configuration, "ProfilesAPI", "/api/profiles")
  .WithAccessToken(RequiredTokenType.Client);


if (config.Apis.Any())
{
    foreach (var api in config.Apis)
    {
        var remoteUri = new Uri(api.RemoteUrl!);

        app.MapRemoteBffApiEndpoint(api.PathMatch, remoteUri)
           .WithAccessToken(api.RequiredToken);
    }
}




app.Run();
