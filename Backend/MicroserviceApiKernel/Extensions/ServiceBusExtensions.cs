using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroserviceApiKernel.Extensions;

public static class ServiceBusExtensions
{
    public static IBusRegistrationConfigurator UseOutbox<T>(this IBusRegistrationConfigurator configurator, Action<IEntityFrameworkOutboxConfigurator> providerConfigurator)
        where T: DbContext
    {
        configurator.AddEntityFrameworkOutbox<T>(o =>
        {
            providerConfigurator.Invoke(o);
            o.UseBusOutbox();
        });
        configurator.AddConfigureEndpointsCallback((context, name, cfg) => 
        { 
            cfg.UseEntityFrameworkOutbox<T>(context); 
        });

        return configurator;
    }
}