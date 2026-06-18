using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using ServiceDefaults;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddAuthorizationDefaultsWithAspire();

builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddAuthorizationPolicies();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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
