using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;

public interface IUserCreateManager
{
    Task<IdentityUser> CreateClientExternal(string email);
    Task<IdentityUser> CreateExternal(string email, string[] roles);
    Task<(IdentityUser, IdentityResult)> CreateInternal(string email, string password, string[] roles);
}

public class UserCreateManager(
    IUserRoleManager roleManager, UserManager<IdentityUser> userManager) : IUserCreateManager
{
    public const string ClientRole = "client";

    public async Task<IdentityUser> CreateClientExternal(string email)
    {
        return await CreateExternal(email, [ClientRole]);
    }
    public async Task<IdentityUser> CreateExternal(string email, string[] roles)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Error while creating user: {createResult.Errors.First().Description}");
        }

        foreach (var role in roles)
            await roleManager.AddUserToRole(user, role);

        return user;
    }

    public async Task<(IdentityUser, IdentityResult)> CreateInternal(string email, string password, string[] roles)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (user, createResult);
        }

        foreach (var role in roles)
            await roleManager.AddUserToRole(user, role);
        
        return (user, createResult);
        
    }
}
