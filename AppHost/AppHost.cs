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

var mongo = builder.AddMongoDB("mongo", 53460)
       .WithDataVolume()
       .WithLifetime(ContainerLifetime.Persistent);

var documentsdb = mongo.AddDatabase("documentsdb");

var identityServer = builder.AddProject<Projects.Deunde_IdentityServer>("IdentityServer")
       .WithHttpsEndpoint(port: 6001)
       .WithExternalHttpEndpoints();

var documentsApi = builder.AddProject<Projects.DocumentsAPI>("DocumentsAPI")
       .WithReference(identityServer)
       .WithReference(blobs)
       .WithReference(documentsdb)
       .WithReference(cache)
       .WithExternalHttpEndpoints();


builder.Build().Run();
