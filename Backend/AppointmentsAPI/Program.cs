using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);


builder.AddMicroserviceDefaults("/api/appointments");

builder.Services.AddControllers();

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
app.MapDefaultControllerRoute();

app.Run();