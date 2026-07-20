using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using OfficesApi.Infrastructure;
using OfficesApi.Services;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddMicroserviceDefaults("/offices");


BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
builder.AddMongoDBClient(connectionName: "officesdb");
builder.Services.AddScoped<OfficesDbContext>();

builder.AddCredentialsClient<IDocumentsClient, DocumentsClient>("identityclient");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OfficesDbContext>();
    await context.InitializeAsync(CancellationToken.None);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapSwaggerDefaults();
}

app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);
app.UseHttpsRedirection();

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();

app.Run();
