var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var blobs = builder.AddAzureStorage("storage")
       .RunAsEmulator(azurite =>
       {
              azurite.WithDataVolume();
              azurite.WithLifetime(ContainerLifetime.Persistent);
              azurite.WithBlobPort(10000)
                     .WithQueuePort(10001)
                     .WithTablePort(10002);
       })
       .AddBlobs("documentsBlob");


       
var rabbitmqServicesApi = builder.AddRabbitMQ("ServicesApiBus")
                  .WithImage("masstransit/rabbitmq", "latest")
                  .WithLifetime(ContainerLifetime.Persistent)
                  .WithHttpEndpoint(port: 15672, targetPort: 15672, name: "management")
                  .WithUrl(
                        url: "http://127.0.0.1:15672",
                        displayText: "RabbitMQ Dashboard"
                    );

var mongo = builder.AddMongoDB("mongo", 53460)
       .WithDataVolume()
       .WithLifetime(ContainerLifetime.Persistent);

var mongodb = mongo.AddDatabase("officesdb");
var documentsdb = mongo.AddDatabase("documentsdb");

var identityServer = builder.AddProject<Projects.Deunde_IdentityServer>("IdentityServer")
       .WithHttpsEndpoint(port: 6001)
       .WithExternalHttpEndpoints();

var documentsApi = builder.AddProject<Projects.DocumentsAPI>("DocumentsAPI")
       .WithReference(identityServer)
       .WithReference(blobs)
       .WithReference(rabbitmqServicesApi)
       .WithReference(documentsdb)
       .WithReference(cache)
       .WithExternalHttpEndpoints()
       .WaitFor(blobs);

var sqlServer = builder.AddSqlServer("sqlServer")
//       .WithHostPort(58379)
       .WithDataVolume()
       .WithLifetime(ContainerLifetime.Persistent);
var profilesDb = sqlServer.AddDatabase("profilesSqlServer");

var profilesAPI = builder.AddProject<Projects.ProfilesAPI>("ProfilesAPI")
       .WithReference(identityServer)
       .WithReference(rabbitmqServicesApi)
       .WithReference(documentsApi)
       .WithReference(profilesDb)
       .WithExternalHttpEndpoints()
       .WaitFor(profilesDb);


var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres")
       .WithHostPort(5432)
       .WithPassword(postgresPassword)
       .WithDataVolume();
var postgresdb = postgres.AddDatabase("appointmentsApiDb");
var servicesApiDb = postgres.AddDatabase("servicesApiDb");

var servicesAPI = builder.AddProject<Projects.ServicesAPI>("ServicesAPI")
       .WithReference(identityServer)
       .WithReference(servicesApiDb)
       .WithReference(rabbitmqServicesApi)
       .WithExternalHttpEndpoints()
       .WaitFor(servicesApiDb)
       .WaitFor(rabbitmqServicesApi);

var appointmentsAPI = builder.AddProject<Projects.AppointmentsAPI>("AppointmentsAPI")
       .WithReference(identityServer)
       .WithReference(postgresdb)
       .WithReference(rabbitmqServicesApi)
       .WithReference(servicesAPI)
       .WithReference(profilesAPI)
       .WithExternalHttpEndpoints()
       .WaitFor(postgresdb);


var officesAPI = builder.AddProject<Projects.OfficesApi>("OfficesAPI")
       .WithReference(identityServer)
       .WithReference(mongodb)
       .WithReference(documentsApi)
       .WaitFor(mongodb)
       .WithExternalHttpEndpoints();


var bff = builder.AddProject<Projects.BFF_FrontendProxy>("BffProxy")
       .WithHttpEndpoint(port: 5000, isProxied: false)
       .WithHttpsEndpoint(port: 5001, isProxied: false)
       .WithReference(identityServer)
       .WithReference(officesAPI)
       .WithReference(profilesAPI)
       .WithReference(servicesAPI)
       .WithReference(appointmentsAPI)
       .WithReference(documentsApi)
       .WithExternalHttpEndpoints();

var frontend = builder.AddViteApp("vite-frontend", "../Frontend/clinic-web-app-frontend")
       .WithHttpEndpoint(port: 5173, isProxied: true)
       .WithEnvironment("VITE_BFF_PROXY_URL", bff.GetEndpoint("https"))
       .WithReference(bff);

bff.WithReference(frontend);

identityServer.WithReference(bff);

profilesAPI.WithReference(bff);

builder.Build().Run();
