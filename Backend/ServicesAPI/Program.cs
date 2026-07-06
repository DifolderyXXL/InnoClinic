using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServiceDefaults;
using ServicesAPI.Application.Scheduling;
using ServicesAPI.Consumers;
using ServicesAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddMicroserviceDefaults("/api/services");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextPool<ServicesDbContext>(options => options.UseSqlite(connectionString).UseLazyLoadingProxies());
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IReservedTimeWindowStore, ReservedTimeWindowStore>();

builder.Services.Configure<ScheduleOptions>(
    builder.Configuration.GetSection(ScheduleOptions.SectionName));
builder.Services.AddScoped<IScheduleSlotsProvider>(
    x=>x.GetRequiredService<IOptions<ScheduleOptions>>().Value);

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