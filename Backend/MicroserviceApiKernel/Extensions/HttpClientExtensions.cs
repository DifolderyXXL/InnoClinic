using Duende.AccessTokenManagement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MicroserviceApiKernel.Extensions;

public static class HttpClientExtensions
{
    private static void ConfigureClient(HttpClient client, IConfiguration configuration, string name)
    {
        var baseAddress = configuration[$"Clients:{name}:BaseAddress"];
        if (!string.IsNullOrEmpty(baseAddress))
        {
            client.BaseAddress = new Uri(baseAddress);
        }
    }

    private static void RegisterClientFromSection(this IHostApplicationBuilder builder, string name)
    {
        builder.Services.AddClientCredentialsTokenManagement()
            .AddClient(name, client =>
            {
                builder.Configuration.GetSection($"Clients:{name}").Bind(client);
            });
    }
    public static void AddCredentialsClient(this IHostApplicationBuilder builder, string name)
    {
        builder.RegisterClientFromSection(name);

        builder.Services.AddClientCredentialsHttpClient(
            name, 
            ClientCredentialsClientName.Parse(name), 
            client => ConfigureClient(client, builder.Configuration, name));
    }
    
    public static void AddCredentialsClient<TInterface, TImplementation>(this IHostApplicationBuilder builder, string name) 
        where TInterface : class where TImplementation : class, TInterface
    {
        builder.RegisterClientFromSection(name);

        builder.Services.AddHttpClient<TInterface, TImplementation>(
                client => ConfigureClient(client, builder.Configuration, name))
            .AddClientCredentialsTokenHandler(ClientCredentialsClientName.Parse(name));
    }
}