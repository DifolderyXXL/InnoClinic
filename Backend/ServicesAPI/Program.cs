using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;
using ServicesAPI.Consumers;
using ServicesAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddMicroserviceDefaults("/api/services");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextPool<ServicesDbContext>(options => options.UseSqlite(connectionString).UseLazyLoadingProxies());

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProcessReservationConsumer>();
    x.AddConsumer<ProcessReservationConfirmationConsumer>();
    x.AddConsumer<CancelReservationConsumer>();
    
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapSwaggerDefaults();
}

app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();

app.Run();