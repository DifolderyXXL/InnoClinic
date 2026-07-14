using Deunde.IdentityServer.Services;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Deunde.IdentityServer;

public class SeedData
{
    public static void EnsureSeedData(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context.Database.Migrate();
            EnsureSeedData(context);
        }
    }
    
    public static void EnsureSeedAdmins(WebApplication app)
    {
        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            var createManager = scope.ServiceProvider.GetRequiredService<IUserCreateManager>();
            var logger = scope.ServiceProvider.GetService<ILogger<SeedData>>();

            var admins = app.Configuration.GetSection(AdminSeedConfiguration.SectionName)
                .Get<List<AdminSeedConfiguration>>() ?? [];

            foreach (var admin in admins)
            {
                try
                {
                    createManager.CreateInternal(admin.Email, admin.Password, admin.Roles);
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Error while seeding {email}", admin.Email);
                }
            }
            
        }
    }

    private static void EnsureSeedData(ConfigurationDbContext context)
    {
        if (!context.IdentityResources.Any())
        {
            Log.Debug("IdentityResources being populated");
            foreach (var resource in Config.IdentityResources.ToList())
            {
                context.IdentityResources.Add(resource.ToEntity());
            }
            context.SaveChanges();
        }
        else
        {
            Log.Debug("IdentityResources already populated");
        }
    }
}

public class AdminSeedConfiguration
{
    public const string SectionName = "AdminSeed";
    
    public string Email { get; set; }
    public string Password { get; set; }
    public string[] Roles { get; set; }
}
