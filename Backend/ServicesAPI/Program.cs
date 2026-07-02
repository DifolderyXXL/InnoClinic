using MassTransit;
using MicroserviceApiKernel;
using MicroserviceApiKernel.Extensions;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;
using ServicesAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHandlers(typeof(Program).Assembly);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.AddOpenApiReversedThroughProxy("/api/services");
builder.AddSwaggerDefaults();
builder.AddAuthorizationDefaultsWithAspire();

builder.Services.AddEndpoints(typeof(Program).Assembly);
builder.Services.AddApiAuthorizationPolicies();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextPool<ServicesDbContext>(options => options.UseSqlite(connectionString).UseLazyLoadingProxies());


builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("ServicesApiBus"));
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapSwaggerDefaults();
}


app.UseCors(PolicyConstants.FRONTEND_BFF_CORS_POLICY);

app.UseAuthorizationDefaultsWithAspire();

app.MapEndpoints();

app.Run();