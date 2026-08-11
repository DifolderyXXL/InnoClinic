using Deunde.IdentityServer.Services.SMTP;
using Infrastructure.Mailing.SMTP;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Worker.Data;
using NotificationService.Worker.Services;
using ServiceDefaults;
using  MicroserviceApiKernel.Extensions;
var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("notificationDb")));

builder.Services.Configure<SmtpClientOptions>(
    builder.Configuration.GetSection(SmtpClientOptions.SectionName));
builder.Services.AddScoped<ISmtpClient, BasicSmtpClient>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    
    x.AddEntityFrameworkOutbox<NotificationContext>(o =>
    {
        o.UsePostgres();

        o.UseBusOutbox(b => b.DisableDeliveryService()); 
    });
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));
        cfg.ConfigureEndpoints(context);
    });
});

builder.AddCredentialsClient<AppointmentApiClient, AppointmentApiClient>("documentsApi");

var host = builder.Build();
host.Run();