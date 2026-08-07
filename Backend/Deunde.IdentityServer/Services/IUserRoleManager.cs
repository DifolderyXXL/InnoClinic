using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;


public interface IUserRoleManager
{
    public Task AddUserToRole(IdentityUser user, string role);
    public Task AddUserToRoles(IdentityUser user, string[] roles);
}

public class UserRoleManager(
    RoleManager<IdentityRole> roleManager,
    UserManager<IdentityUser> userManager) : IUserRoleManager
{
    public async Task AddUserToRole(IdentityUser user, string role)
    {
        await RoleHelper.EnsureRole(roleManager, role);

        var addToRoleResult = await userManager.AddToRoleAsync(user, role);

        if (!addToRoleResult.Succeeded)
        {
            var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Cant add role '{role}': {errors}");
        }
    }

    public async Task AddUserToRoles(IdentityUser user, string[] roles)
    {
        if (roles.Length == 0) return;

        foreach (var role in roles.Distinct())
        {
            await RoleHelper.EnsureRole(roleManager, role);
        }

        var addToRolesResult = await userManager.AddToRolesAsync(user, roles);

        if (!addToRolesResult.Succeeded)
        {
            var errors = string.Join(", ", addToRolesResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Cant add roles [{string.Join(", ", roles)}]: {errors}");
        }
    }
}

public class RoleHelper
{
    public static async Task EnsureRole(RoleManager<IdentityRole> roleManager, string role)
    {
        if (string.IsNullOrWhiteSpace(role)) return;

        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create role '{role}': {errors}");
            }
        }
    }
}