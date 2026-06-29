
var builder = DistributedApplication.CreateBuilder(args);

var identityServer = builder.AddProject<Projects.Deunde_IdentityServer>("IdentityServer")
       .WithHttpsEndpoint(port: 6001)
       .WithExternalHttpEndpoints();

var profilesAPI = builder.AddProject<Projects.ProfilesAPI>("ProfilesAPI")
       .WithReference(identityServer)
       .WithExternalHttpEndpoints();

var servicesAPI = builder.AddProject<Projects.ServicesAPI>("ServicesAPI")
       .WithReference(identityServer)
       .WithExternalHttpEndpoints();


var mongo = builder.AddMongoDB("mongo", 53460)
                   .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("officesdb");

var officesAPI = builder.AddProject<Projects.OfficesApi>("OfficesAPI")
       .WithReference(identityServer)
       .WithReference(mongodb)
       .WaitFor(mongodb)
       .WithExternalHttpEndpoints();


var bff = builder.AddProject<Projects.BFF_FrontendProxy>("BffProxy")
       .WithHttpEndpoint(port: 5000, isProxied: false)
       .WithHttpsEndpoint(port: 5001, isProxied: false)
       .WithReference(identityServer)
       .WithReference(officesAPI)
       .WithReference(profilesAPI)
       .WithReference(servicesAPI)
       .WithExternalHttpEndpoints();

var frontend = builder.AddViteApp("vite-frontend", "../Frontend/clinic-web-app-frontend")
       .WithHttpEndpoint(port: 5173)
       .WithEnvironment("VITE_BFF_PROXY_URL", bff.GetEndpoint("https"))
       .WithReference(bff);

bff.WithReference(frontend);

identityServer.WithReference(bff);


builder.Build().Run();
