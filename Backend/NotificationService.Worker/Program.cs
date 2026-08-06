using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Worker.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("notificationDb")));

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

var host = builder.Build();
host.Run();