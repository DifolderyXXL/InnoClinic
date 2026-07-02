using Yarp.ReverseProxy.Forwarder;
using System.Net;
using System.Diagnostics;

internal static class ViteDevServerProxy
{
    private static readonly HttpMessageInvoker ViteProxyClient = new(
        new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.All,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2)
        }
    );

    public static async Task ForwardRequestAsync(IHttpForwarder forwarder, HttpContext context, string destinationPrefix)
    {
        var requestConfig = new ForwarderRequestConfig
        {
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        if (context.Request.Path == "/")
        {
            context.Request.Path = "/index.html";
        }

        await forwarder.SendAsync(
            context,
            destinationPrefix,
            ViteProxyClient,
            requestConfig,
            HttpTransformer.Default
        );
    }
}
