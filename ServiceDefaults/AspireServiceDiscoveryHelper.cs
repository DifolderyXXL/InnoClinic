using Microsoft.Extensions.Configuration;

public static class AspireServiceDiscoveryHelper
{
    extension(IConfiguration configuration)
    {
        public string? DiscoverHttp(string name)
            => configuration[$"services:{name}:http:0"];


        public string? DiscoverHttps(string name)
            => configuration[$"services:{name}:https:0"];


        public string? DiscoverAny(string name)
            => configuration.DiscoverHttps(name) ?? configuration.DiscoverHttp(name);
    }

}
