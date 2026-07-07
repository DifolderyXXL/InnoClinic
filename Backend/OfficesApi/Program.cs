using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using OfficesApi.Infrastructure;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddMicroserviceDefaults("/offices");


builder.AddMongoDBClient(connectionName: "officesdb");
builder.Services.AddScoped<OfficesDbContext>();


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
