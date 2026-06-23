
var builder = DistributedApplication.CreateBuilder(args);

var identityServer = builder.AddProject<Projects.Deunde_IdentityServer>("IdentityServer")
       .WithHttpsEndpoint(port: 6001)
       .WithExternalHttpEndpoints();

var profilesAPI = builder.AddProject<Projects.ProfilesAPI>("ProfilesAPI")
       .WithReference(identityServer)
       .WithExternalHttpEndpoints();

var officesAPI = builder.AddProject<Projects.OfficesApi>("OfficesAPI")
       .WithReference(identityServer)
       .WithExternalHttpEndpoints();


var bff = builder.AddProject<Projects.BFF_FrontendProxy>("BffProxy")
       .WithHttpEndpoint(port: 5000, isProxied: false)
       .WithHttpsEndpoint(port: 5001, isProxied: false)
       .WithReference(identityServer)
       .WithReference(officesAPI)
       .WithReference(profilesAPI)
       .WithExternalHttpEndpoints(); ;


identityServer.WithReference(bff);

var frontend = builder.AddViteApp("vite-frontend", "../Frontend/clinic-web-app-frontend")
       .WithHttpEndpoint(port: 5173)
       .WithEnvironment("VITE_BFF_PROXY_URL", bff.GetEndpoint("https"))
       .WithReference(bff);


builder.Build().Run();
