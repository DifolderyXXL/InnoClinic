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

builder.AddMicroserviceDefaults("/api/services", typeof(Program).Assembly);

builder.Services.AddDbContext<ServicesDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("servicesApiDb")).UseLazyLoadingProxies());

builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservedTimeWindowStore, ReservedTimeWindowStore>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();

builder.Services.Configure<ReservationOptions>(
    builder.Configuration.GetSection(ReservationOptions.SectionName));

builder.Services.Configure<ScheduleOptions>(
    builder.Configuration.GetSection(ScheduleOptions.SectionName));
builder.Services.AddScoped<IScheduleSlotsProvider>(
    x=>x.GetRequiredService<IOptions<ScheduleOptions>>().Value);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<ServicesDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddConfigureEndpointsCallback((context, name, cfg) => 
    { 
        cfg.UseEntityFrameworkOutbox<ServicesDbContext>(context); 
    });

    x.AddConsumer<ProcessReservationConsumer>();
    x.AddConsumer<ProcessReservationConfirmationConsumer>();
    
    x.AddConsumer<CancelReservationConsumer>();
    x.AddConsumer<ReservationExpiredConsumer>();
    
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));
        cfg.UseDelayedMessageScheduler();
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