using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OTel;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.OpenApi;
using ProfilesAPI.Data;
using ServiceDefaults;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHandlers(typeof(Program).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi(options =>
// {
//     options.AddDocumentTransformer((document, context, cancellationToken) =>
//     {
//         document.Servers.Clear();
//         document.Servers.Add(new()
//         {
//             Url = "https://localhost:5001/api/profiles"
//         });
//         return Task.CompletedTask;
//     });
// });

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Ensure instances exist
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();



        // Add OAuth2 security scheme (Authorization Code flow only)
        document.Components.SecuritySchemes.Add("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri("https://localhost:6001/connect/authorize"),
                    TokenUrl = new Uri("https://localhost:6001/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        { "api", "Access the Weather API" },
                        { "openid", "Access the OpenID Connect user profile" },
                        { "email", "Access the user's email address" },
                        { "profile", "Access the user's profile" }
                    }
                }
            }
        });

        // Apply security requirement globally
        document.Security = [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("oauth2"),
                    ["api", "profile", "email", "openid"]
                }
            }
        ];

        // Set the host document for all elements
        // including the security scheme references
        document.SetReferenceHostDocument();

        return Task.CompletedTask;
    });
});

builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(i => i.FullName?.Replace('+', '_'));

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:6001/connect/authorize"),
                TokenUrl = new Uri("https://localhost:6001/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "api", "Access the Weather API" },
                    { "openid", "Access the OpenID Connect user profile" },
                    { "email", "Access the user's email address" },
                    { "profile", "Access the user's profile" }
                }
            }
        }
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("oauth2", document),
            new List<string> { "api", "profile", "email", "openid" }
        }
    });
});
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


    app.MapSwagger();
    app.MapSwaggerUI(setupAction: options =>
    {

        options.UseRequestInterceptor("function(request){ request.headers['X-CSRF'] = '1';return request;}");
        options.OAuthUsePkce();

    });
}



app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);

app.UseHttpsRedirection();

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();

app.MapGet("/my-profile", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value
              ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var email = user.FindFirst("email")?.Value
             ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    return Results.Ok(new { Message = "Okey, we have profile", UserId = userId, Email = email });
}).RequireAuthorization();

app.MapGet("/client-only", (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("sub")?.Value
              ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

    var email = user.FindFirst("email")?.Value
             ?? user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

    return Results.Ok(new { Message = "You are only client, ok!", UserId = userId, Email = email });
}).RequireAuthorization(RolePolicy.Client);

app.MapGet("/get-headers", (HttpContext context) =>
{
    return Results.Ok(context.Request.Headers.Select(x => $"{x.Key}: {x.Value}").ToArray());
});



app.Run();
