using AuthorizationAPI.Data;
using AuthorizationAPI.Extensions;
using AuthorizationAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ServiceDefaults;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

//builder.AddSqlServerClient(connectionName: "AuthDB");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=app.db")); //builder.Configuration.GetConnectionString("AuthDB")

builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    //options.LoginPath = "/Identity/Account/Login";
    //options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
});



builder.Services.AddAuthorization();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddEndpoints(typeof(Program).Assembly);

builder.Services.AddScoped<ISessionAuthorizeManager, SessionAuthorizeManager>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IConfirmationSender, LogsConfirmationSender>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();
}


app.MapEndpoints();
app.MapDefaultEndpoints();

app.UseCors(PolicyConstants.FRONTEND_CORS_POLICY);

app.UseAuthentication();
app.UseAuthorization();


app.MapGroup("auth").MapIdentityApi<IdentityUser>();

app.MapSwagger();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/weatherforecast", async (ClaimsPrincipal user) =>
{
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var email = user.FindFirst(ClaimTypes.Email)?.Value;

    return $"ZAEBISCHE {userId}: {email}";
}).RequireAuthorization();


app.Run();