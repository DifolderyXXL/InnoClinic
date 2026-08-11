using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDefaults;

public interface IGatewayUrlProvider
{
    public string BaseUrl { get; }
}

public class GatewayUrlProvider(string baseUrl) : IGatewayUrlProvider
{
    public string BaseUrl { get; } = baseUrl;
}

public static class GatewayUrlProviderExtension
{
    public static IServiceCollection AddGatewayUrlProvider(this IServiceCollection services)
        => services.AddSingleton<IGatewayUrlProvider>(sp => {
            var config = sp.GetRequiredService<IConfiguration>();
            var gatewayBaseUrl = config.DiscoverHttps("BffProxy");

            if (string.IsNullOrWhiteSpace(gatewayBaseUrl))
            {
                throw new InvalidOperationException("BffProxy URL not found.");
            }
    
            return new GatewayUrlProvider(gatewayBaseUrl);
        });

}
