using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;


public interface IUserRoleManager
{
    public Task AddUserToRole(IdentityUser user, string role);
}

public class UserRoleManager(
    RoleManager<IdentityRole> roleManager,
    UserManager<IdentityUser> userManager) : IUserRoleManager
{
    public async Task AddUserToRole(IdentityUser user, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, role);

        if (!addToRoleResult.Succeeded)
        {
            throw new InvalidOperationException($"Cant add role: {addToRoleResult.Errors.First().Description}");
        }
    }
}

public class RoleHelper
{
    public static async Task EnsureRole(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}