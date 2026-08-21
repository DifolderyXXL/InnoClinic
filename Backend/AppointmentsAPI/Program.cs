using AppointmentsAPI.Consumers;
using AppointmentsAPI.Controllers;
using AppointmentsAPI.Data;
using AppointmentsAPI.Services;
using Contracts.AppointmentContracts;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);


builder.AddMicroserviceDefaults("/appointments");
builder.Services.AddIdentityAuthorizationPolicies();

builder.Services.AddControllers();

builder.Services.AddScoped<IAppointmentService, AppointmentService>();

builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("appointmentsApiDb")));

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<AppointmentStateMachine, AppointmentState, AppointmentSagaDefinition>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.ExistingDbContext<AppointmentDbContext>();
            r.UsePostgres();
        });
    x.AddEntityFrameworkOutbox<AppointmentDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    
    x.AddConsumer<AppointmentConfirmedConsumer>();
    x.AddConsumer<AppointmentStateChangedConsumer>();
    x.AddConsumer<AppointmentTimeWindowReservedSyncConsumer>();
    x.AddConsumer<AppointmentRescheduledConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));
        cfg.ConfigureEndpoints(context);
    });
});

builder.AddCredentialsClient<IProfilesApiClient, ProfilesApiClient>("ProfilesApiClient");
builder.Services.AddHttpClient<IServicesApiClient, ServicesApiClient>(client =>
{
    client.BaseAddress = new Uri("https://ServicesAPI");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapSwaggerDefaults();
}

app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();
app.MapDefaultControllerRoute();

app.Run();

public class AppointmentSagaDefinition : SagaDefinition<AppointmentState>
{
    protected override void ConfigureSaga(
        IReceiveEndpointConfigurator endpointConfigurator, 
        ISagaConfigurator<AppointmentState> sagaConfigurator, 
        IRegistrationContext context)
    {
        endpointConfigurator.UseMessageRetry(r => r.Interval(5, TimeSpan.FromMilliseconds(100)));

        endpointConfigurator.UseEntityFrameworkOutbox<AppointmentDbContext>(context);
    }
}