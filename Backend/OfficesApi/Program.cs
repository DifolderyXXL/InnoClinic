using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using OfficesApi.Infrastructure;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddMongoDBClient(connectionName: "officesdb");

builder.Services.AddHandlers(typeof(Program).Assembly);
builder.Services.AddScoped<OfficesDbContext>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddOpenApiReversedThroughProxy("/api/offices");

builder.AddSwaggerDefaults();
builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddValidation(typeof(Program).Assembly);
builder.AddServiceDefaults();

builder.AddAuthorizationDefaultsWithAspire();

builder.Services.AddApiAuthorizationPolicies();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OfficesDbContext>();
    await context.InitializeAsync(default);
}

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
