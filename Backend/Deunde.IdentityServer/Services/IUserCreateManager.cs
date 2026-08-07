using Microsoft.AspNetCore.Identity;

namespace Deunde.IdentityServer.Services;

public interface IUserCreateManager
{
    Task<IdentityUser> CreateClientExternal(string email, bool isExternalEmailVerified);
    Task<IdentityUser> CreateExternal(string email, string[] roles, bool isExternalEmailVerified);
    Task<(IdentityUser, IdentityResult)> CreateInternal(string email, string password, string[] roles);
}

public class UserCreateManager(
    IUserRoleManager roleManager, UserManager<IdentityUser> userManager) : IUserCreateManager
{
    public const string ClientRole = "client";

    public async Task<IdentityUser> CreateClientExternal(string email, bool isExternalEmailVerified)
    {
        return await CreateExternal(email, [ClientRole], isExternalEmailVerified);
    }
    public async Task<IdentityUser> CreateExternal(string email, string[] roles, bool isExternalEmailVerified)
    {
        if (roles.Length == 0) 
            throw new ArgumentException("Minimum one role is required", nameof(roles));
        
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = isExternalEmailVerified
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Error while creating user: {errors}");
        }
        
        await roleManager.AddUserToRoles(user, roles);

        return user;
    }

    public async Task<(IdentityUser, IdentityResult)> CreateInternal(string email, string password, string[] roles)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            return (user, createResult);
        }

        await roleManager.AddUserToRoles(user, roles);
        
        return (user, createResult);
        
    }
}
