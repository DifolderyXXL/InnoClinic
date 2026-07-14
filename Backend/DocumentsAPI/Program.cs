using DocumentsAPI.Consumers;
using DocumentsAPI.Data;
using DocumentsAPI.Infrastructure;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureBlobServiceClient("documentsBlob");
builder.Services.AddScoped<BlobDbContext>();

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
builder.AddMongoDBClient(connectionName: "documentsdb");
builder.Services.AddScoped<MedicalResultsDbContext>();

builder.Services.AddScoped<ProfilePhotoRepository>();
builder.Services.AddScoped<PublicPhotoRepository>();

builder.Services.AddScoped<IUserPhotoStorage, UserPhotoStorage>();
builder.Services.AddScoped<IPublicPhotoStorage, PublicPhotoStorage>();

builder.AddMicroserviceDefaults("/documents");
builder.Services.AddIdentityAuthorizationPolicies();
builder.Services.AddControllers();

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    x.AddConsumer<ConfirmProfilePhotoConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));

        cfg.ConfigureEndpoints(context);
    });
});



var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BlobDbContext>();
    await context.EnsureCreated(CancellationToken.None);

    var medicalResultContext = scope.ServiceProvider.GetRequiredService<MedicalResultsDbContext>();
    await medicalResultContext.InitializeAsync(CancellationToken.None);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapSwaggerDefaults();
}

app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);
//app.UseHttpsRedirection();

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();
app.MapDefaultControllerRoute();

app.Run();