using DocumentsAPI.Consumers;
using DocumentsAPI.Infrastructure;
using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureBlobServiceClient("documentsBlob");
builder.Services.AddScoped<BlobDbContext>();
builder.Services.AddScoped<PhotoRepository>();
builder.Services.AddScoped<ITempPhotoStorage, BlobTempPhotoStorage>();
builder.Services.AddScoped<IProfilePhotoStorage, BlobProfilePhotoStorage>();

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