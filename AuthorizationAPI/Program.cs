
using AuthorizationAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

const string ALLOWCORS_POLICY = "ALLOWCORS_POLICY";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db"));


builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy(ALLOWCORS_POLICY, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseCors(ALLOWCORS_POLICY);
app.MapGroup("auth").MapIdentityApi<IdentityUser>();

app.MapSwagger().RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();