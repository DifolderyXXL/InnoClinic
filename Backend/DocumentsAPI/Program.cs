using DocumentsAPI;
using DocumentsAPI.Consumers;
using DocumentsAPI.Infrastructure;
using DocumentsAPI.Options;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using QuestPDF.Infrastructure;
using ServiceDefaults;

QuestPDF.Settings.License = LicenseType.Evaluation;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureBlobServiceClient("documentsBlob");

BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
builder.AddMongoDBClient(connectionName: "documentsdb");

builder.Services.Configure<PdfGenerationLockOptions>(
    builder.Configuration.GetSection(PdfGenerationLockOptions.SectionName));

builder.AddMicroserviceDefaults("/documents");
builder.Services.AddIdentityAuthorizationPolicies();
builder.Services.AddControllers();

builder.Services.AddServices();

builder.AddRedisClient(connectionName: "cache");

builder.Services.AddMassTransit(x =>
{
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