using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddOpenApiReversedThroughProxy("/api/offices");

builder.AddSwaggerDefaults();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.AddServiceDefaults();

builder.AddAuthorizationDefaultsWithAspire();

builder.Services.AddApiAuthorizationPolicies();

var app = builder.Build();

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

app.Run();
