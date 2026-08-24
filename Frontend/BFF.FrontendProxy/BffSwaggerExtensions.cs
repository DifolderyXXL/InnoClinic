namespace BFF.FrontendProxy;

public static class BffSwaggerExtensions
{
    public static void UseBffGlobalSwagger(this WebApplication app, List<MicroserviceDefenition> definitions)
    {
        app.MapSwaggerUI(setupAction: options =>
            {
                foreach (var definition in definitions)
                {
                    var serviceName = definition.Name;
                    var host = app.Configuration.DiscoverAny(definition.Name);
                    foreach (var version in definition.SupportedVersions)
                    {
                        options.SwaggerEndpoint(
                            $"/swagger-proxy/{serviceName}/openapi/{version}.json",
                            $"{definition.Name} {version}"
                        );
                    }
                }

                options.ConfigObject.AdditionalItems["withCredentials"] = true;
                options.UseRequestInterceptor("function(request){ request.headers['X-CSRF'] = '1';return request;}");
            })
            .AllowAnonymous();

        app.MapGet("/swagger-proxy/{serviceName}/{**path}", async (
            string serviceName,
            string path,
            IConfiguration config,
            IHttpClientFactory httpClientFactory) =>
        {
            var host = config.DiscoverAny(serviceName);
            var client = httpClientFactory.CreateClient();

            var response = await client.GetAsync($"{host}/{path}");

            if (!response.IsSuccessStatusCode)
                return Results.StatusCode((int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            return Results.Content(json, "application/json");
        });
    }
}